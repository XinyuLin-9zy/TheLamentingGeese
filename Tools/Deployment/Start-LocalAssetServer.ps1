param(
    [string]$Root = ".\Deploy\AssetServer",
    [int]$Port = 8000
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $Root)) {
    New-Item -ItemType Directory -Path $Root -Force | Out-Null
}

$rootFull = (Resolve-Path -LiteralPath $Root).Path
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    $python = Get-Command py -ErrorAction SilentlyContinue
}
if (-not $python) {
    throw "Python was not found. Install Python or start another static file server for $rootFull"
}

$pidPath = Join-Path (Resolve-Path -LiteralPath ".").Path "Deploy\asset-server.pid"
New-Item -ItemType Directory -Path (Split-Path -Parent $pidPath) -Force | Out-Null
$outLog = Join-Path (Split-Path -Parent $pidPath) "asset-server.out.log"
$errLog = Join-Path (Split-Path -Parent $pidPath) "asset-server.err.log"

$argumentList = @(
    "-m",
    "http.server",
    $Port.ToString(),
    "--bind",
    "0.0.0.0",
    "--directory",
    ('"{0}"' -f $rootFull)
)
$process = Start-Process -FilePath $python.Source -ArgumentList $argumentList -WindowStyle Hidden -RedirectStandardOutput $outLog -RedirectStandardError $errLog -PassThru
Start-Sleep -Milliseconds 500
if ($process.HasExited) {
    $errorText = if (Test-Path -LiteralPath $errLog) { Get-Content -LiteralPath $errLog -Raw } else { "" }
    throw "Asset server exited immediately with code $($process.ExitCode). $errorText"
}

Set-Content -LiteralPath $pidPath -Value $process.Id -Encoding ASCII

$addresses = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
    Where-Object { $_.IPAddress -notlike "127.*" -and $_.IPAddress -notlike "169.254.*" } |
    Select-Object -ExpandProperty IPAddress

[pscustomobject]@{
    ProcessId = $process.Id
    Root = $rootFull
    LocalUrl = "http://127.0.0.1:$Port/"
    LanUrls = ($addresses | ForEach-Object { "http://$_`:$Port/" }) -join "; "
    PidFile = $pidPath
} | Format-List
