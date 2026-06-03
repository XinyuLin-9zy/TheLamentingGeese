param(
    [string]$SourceRoot = ".\BuildAssetBundles",
    [string]$OutputRoot = ".\Deploy\AssetServer",
    [string]$GameId = "theweepingswan",
    [string]$Version = "1.0.0",
    [string[]]$Platforms = @("Android", "iOS"),
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

function Resolve-ExistingPath([string]$PathValue, [string]$Label) {
    if (-not (Test-Path -LiteralPath $PathValue)) {
        throw "$Label does not exist: $PathValue"
    }
    return (Resolve-Path -LiteralPath $PathValue).Path
}

$workspace = (Resolve-Path -LiteralPath ".").Path
$sourceRootFull = Resolve-ExistingPath $SourceRoot "SourceRoot"
$outputRootFull = if (Test-Path -LiteralPath $OutputRoot) {
    (Resolve-Path -LiteralPath $OutputRoot).Path
} else {
    New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null
    (Resolve-Path -LiteralPath $OutputRoot).Path
}

$versionRoot = Join-Path (Join-Path $outputRootFull $GameId) $Version

if ($Clean -and (Test-Path -LiteralPath $versionRoot)) {
    $resolvedVersionRoot = (Resolve-Path -LiteralPath $versionRoot).Path
    if (-not $resolvedVersionRoot.StartsWith($outputRootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean outside OutputRoot: $resolvedVersionRoot"
    }
    Remove-Item -LiteralPath $resolvedVersionRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $versionRoot -Force | Out-Null

$results = foreach ($platform in $Platforms) {
    $srcPlatform = Join-Path $sourceRootFull $platform
    if (-not (Test-Path -LiteralPath $srcPlatform)) {
        [pscustomobject]@{
            Platform = $platform
            Status = "MissingSource"
            Source = $srcPlatform
            Output = ""
            Manifest = ""
        }
        continue
    }

    $dstPlatform = Join-Path $versionRoot $platform
    New-Item -ItemType Directory -Path $dstPlatform -Force | Out-Null

    Get-ChildItem -LiteralPath $srcPlatform -Force | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $dstPlatform -Recurse -Force
    }

    $manifest = Join-Path $dstPlatform $platform
    $status = if (Test-Path -LiteralPath $manifest) { "Ready" } else { "MissingManifest" }
    [pscustomobject]@{
        Platform = $platform
        Status = $status
        Source = $srcPlatform
        Output = $dstPlatform
        Manifest = $manifest
    }
}

$results | Format-Table -AutoSize

Write-Host ""
Write-Host "ServerUrl should point to the version root, not the platform folder:"
Write-Host "  http://<host>/<game-id>/<version>"
Write-Host "Example for this output:"
Write-Host "  http://<host>/$GameId/$Version"

if ($results.Status -contains "MissingSource" -or $results.Status -contains "MissingManifest") {
    exit 2
}
