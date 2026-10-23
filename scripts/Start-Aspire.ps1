<#
.SYNOPSIS
    Starts Docker Desktop if it isn't running, waits for the engine, then launches the
    eShop.AppHost Aspire orchestrator.
#>
param(
    [int]$DockerWaitSeconds = 120,
    [int]$PortCheckTimeoutSeconds = 10
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$dockerExe = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
$dockerCli = "docker"

function Test-DockerEngine {
    try {
        & $dockerCli info 2>&1 | Out-Null
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

if (-not (Test-DockerEngine)) {
    if (-not (Test-Path $dockerExe)) {
        throw "Docker Desktop executable not found at $dockerExe. Install Docker Desktop first."
    }

    Write-Host "Docker is not running. Starting Docker Desktop..."
    Start-Process -FilePath $dockerExe | Out-Null

    $deadline = (Get-Date).AddSeconds($DockerWaitSeconds)
    do {
        Start-Sleep -Seconds 5
        Write-Host "Waiting for Docker engine..."
    } while (-not (Test-DockerEngine) -and (Get-Date) -lt $deadline)

    if (-not (Test-DockerEngine)) {
        throw "Docker engine did not become ready within $DockerWaitSeconds seconds."
    }
    Write-Host "Docker engine is ready."
} else {
    Write-Host "Docker is already running."
}

Write-Host "Launching eShop.AppHost..."
& dotnet run --project (Join-Path $repoRoot "src/eShop.AppHost/eShop.AppHost.csproj")