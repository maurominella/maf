<#
.SYNOPSIS
    Build a MAF hosted agent Docker image, tag it, and push it to Azure
    Container Registry (ACR).

.DESCRIPTION
    Wraps the three-step workflow (docker build → docker tag → docker push)
    and calls "az acr login" automatically before pushing.

.PARAMETER AcrName
    The short name of your ACR (e.g. "myregistry"), without the
    ".azurecr.io" suffix.

.PARAMETER ImageName
    The name to give the Docker image (e.g. "ha02-azureopenaiagent").

.PARAMETER Tag
    The image tag (default: "latest").

.PARAMETER Context
    Path to the agent folder that contains the Dockerfile (default: ".").

.EXAMPLE
    .\scripts\build_and_push.ps1 `
        -AcrName   "myregistry" `
        -ImageName "ha02-azureopenaiagent" `
        -Tag       "latest" `
        -Context   "agents\ha02-azureopenaiagent"

.NOTES
    Prerequisites:
    - Docker Desktop running
    - Azure CLI installed and authenticated (az login)
    - Your Azure identity must have the AcrPush role on the ACR
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$AcrName,

    [Parameter(Mandatory = $true)]
    [string]$ImageName,

    [string]$Tag = "latest",

    [string]$Context = "."
)

$ErrorActionPreference = "Stop"

$FullImage = "${AcrName}.azurecr.io/${ImageName}:${Tag}"

Write-Host ""
Write-Host "=============================================="
Write-Host "  MAF Hosted Agent — Build & Push to ACR"
Write-Host "=============================================="
Write-Host "  ACR      : ${AcrName}.azurecr.io"
Write-Host "  Image    : ${ImageName}:${Tag}"
Write-Host "  Full tag : ${FullImage}"
Write-Host "  Context  : ${Context}"
Write-Host "----------------------------------------------"

# ---------------------------------------------------------------------------
# Step 1: Authenticate with ACR
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "[1/4] Authenticating with ACR..."
az acr login --name $AcrName
if ($LASTEXITCODE -ne 0) { throw "az acr login failed." }

# ---------------------------------------------------------------------------
# Step 2: Build the image
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "[2/4] Building Docker image..."
docker build --platform linux/amd64 -t "${ImageName}:${Tag}" $Context
if ($LASTEXITCODE -ne 0) { throw "docker build failed." }

# ---------------------------------------------------------------------------
# Step 3: Tag the image for ACR
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "[3/4] Tagging image for ACR..."
docker tag "${ImageName}:${Tag}" $FullImage
if ($LASTEXITCODE -ne 0) { throw "docker tag failed." }

# ---------------------------------------------------------------------------
# Step 4: Push to ACR
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "[4/4] Pushing image to ACR..."
docker push $FullImage
if ($LASTEXITCODE -ne 0) { throw "docker push failed." }

Write-Host ""
Write-Host "=============================================="
Write-Host "  Done!  Image available at:"
Write-Host "  $FullImage"
Write-Host "=============================================="
