param(
    [Parameter(Mandatory = $true)]
    [string]$Bucket,

    [string]$LocalRoot = ".\Deploy\AssetServer",
    [string]$Prefix = ""
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $LocalRoot)) {
    throw "LocalRoot does not exist: $LocalRoot"
}

$wrangler = Get-Command wrangler -ErrorAction SilentlyContinue
if (-not $wrangler) {
    throw "wrangler was not found. Install it with: npm install -g wrangler"
}

$rootFull = (Resolve-Path -LiteralPath $LocalRoot).Path
$files = Get-ChildItem -LiteralPath $rootFull -Recurse -File
if (-not $files) {
    throw "No files found under $rootFull"
}

foreach ($file in $files) {
    $relative = $file.FullName.Substring($rootFull.Length).TrimStart("\", "/") -replace "\\", "/"
    $key = if ([string]::IsNullOrWhiteSpace($Prefix)) { $relative } else { (($Prefix.TrimEnd("/") + "/" + $relative) -replace "\\", "/") }
    Write-Host "Uploading $relative -> r2://$Bucket/$key"
    & $wrangler.Source r2 object put "$Bucket/$key" --file "$($file.FullName)"
    if ($LASTEXITCODE -ne 0) {
        throw "wrangler failed while uploading $relative"
    }
}

Write-Host "Uploaded $($files.Count) files to bucket $Bucket"
