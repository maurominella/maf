<#
.SYNOPSIS
    build_and_push.ps1 — Build a container image and push it to Azure Container Registry (ACR).

.DESCRIPTION
    Automates the Docker build, ACR login, tag, and push steps for a
    Microsoft Foundry Hosted Agent container image.

.PARAMETER Registry
    ACR registry name WITHOUT the .azurecr.io suffix (required).

.PARAMETER Repository
    Image repository / namespace inside ACR (default: hosted-agents).

.PARAMETER Image
    Image name (required).

.PARAMETER Tag
    Image tag (default: latest).

.PARAMETER Context
    Docker build context directory (default: current directory).

.PARAMETER File
    Path to the Dockerfile. Defaults to <Context>/Dockerfile.

.EXAMPLE
    .\scripts\build_and_push.ps1 `
        -Registry   myfoundryacr `
        -Repository hosted-agents `
        -Image      ha01-echoagent `
        -Tag        latest `
        -Context    agents\ha01_echoagent

.NOTES
    Prerequisites:
      - Docker Desktop (or Docker Engine) installed and running
      - Azure CLI installed (az)
      - Logged in to Azure: az login

    No secrets are committed or embedded by this script.
    Pass secrets via environment variables or --env-file at docker run time.
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string] $Registry,

    [Parameter(Mandatory = $false)]
    [string] $Repository = "hosted-agents",

    [Parameter(Mandatory = $true)]
    [string] $Image,

    [Parameter(Mandatory = $false)]
    [string] $Tag = "latest",

    [Parameter(Mandatory = $false)]
    [string] $Context = ".",

    [Parameter(Mandatory = $false)]
    [string] $File = ""
)

$ErrorActionPreference = "Stop"

# ── Derived values ────────────────────────────────────────────────────────────
$LoginServer = "${Registry}.azurecr.io"
$FullImage   = "${LoginServer}/${Repository}/${Image}:${Tag}"

# ── Steps ─────────────────────────────────────────────────────────────────────
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Microsoft Foundry Hosted Agent — Build & Push to ACR"      -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Registry   : $LoginServer"
Write-Host "  Full image : $FullImage"
Write-Host "  Context    : $Context"
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# 1. ACR login
Write-Host "[1/3] Logging in to ACR: $LoginServer ..." -ForegroundColor Yellow
az acr login --name $Registry
if ($LASTEXITCODE -ne 0) { throw "az acr login failed." }
Write-Host ""

# 2. Docker build
Write-Host "[2/3] Building image: $FullImage ..." -ForegroundColor Yellow
$buildArgs = @("build", "--tag", $FullImage)
if ($File -ne "") { $buildArgs += @("--file", $File) }
$buildArgs += $Context

& docker @buildArgs
if ($LASTEXITCODE -ne 0) { throw "docker build failed." }
Write-Host ""

# 3. Docker push
Write-Host "[3/3] Pushing image: $FullImage ..." -ForegroundColor Yellow
docker push $FullImage
if ($LASTEXITCODE -ne 0) { throw "docker push failed." }
Write-Host ""

Write-Host "============================================================" -ForegroundColor Green
Write-Host " Done! Image available at:"                                   -ForegroundColor Green
Write-Host "   $FullImage"                                                -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
