param(
    [string]$PidFile = ".\Deploy\asset-server.pid"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $PidFile)) {
    Write-Host "No PID file found: $PidFile"
    exit 0
}

$pidValue = (Get-Content -LiteralPath $PidFile -Raw).Trim()
if (-not $pidValue) {
    Write-Host "PID file is empty: $PidFile"
    exit 0
}

$process = Get-Process -Id ([int]$pidValue) -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Id $process.Id -Force
    Write-Host "Stopped asset server process $($process.Id)"
} else {
    Write-Host "Asset server process is not running: $pidValue"
}

Remove-Item -LiteralPath $PidFile -Force
