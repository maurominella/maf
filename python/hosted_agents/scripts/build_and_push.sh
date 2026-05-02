#!/usr/bin/env bash
# =============================================================================
# build_and_push.sh
# Build a MAF hosted agent Docker image, tag it, and push it to Azure Container
# Registry (ACR).
#
# Usage:
#   chmod +x scripts/build_and_push.sh
#   scripts/build_and_push.sh \
#     --acr     <acr-name>          \
#     --image   <image-name>        \
#     --tag     <image-tag>         \
#     --context <path-to-agent-dir>
#
# Example:
#   scripts/build_and_push.sh \
#     --acr     myregistry           \
#     --image   ha02-azureopenaiagent \
#     --tag     latest               \
#     --context agents/ha02-azureopenaiagent
#
# Prerequisites:
#   - Docker Desktop (or Docker Engine) running
#   - Azure CLI installed and authenticated (az login)
#   - The Azure identity must have the AcrPush role on the ACR
# =============================================================================

set -euo pipefail

# ---------------------------------------------------------------------------
# Defaults
# ---------------------------------------------------------------------------
ACR_NAME=""
IMAGE_NAME=""
IMAGE_TAG="latest"
BUILD_CONTEXT="."

# ---------------------------------------------------------------------------
# Parse arguments
# ---------------------------------------------------------------------------
while [[ $# -gt 0 ]]; do
  case "$1" in
    --acr)     ACR_NAME="$2";    shift 2 ;;
    --image)   IMAGE_NAME="$2";  shift 2 ;;
    --tag)     IMAGE_TAG="$2";   shift 2 ;;
    --context) BUILD_CONTEXT="$2"; shift 2 ;;
    -h|--help)
      sed -n '/^# Usage/,/^# Prerequisites/{ /^# Prerequisites/!p }' "$0" \
        | sed 's/^# //' | sed 's/^#//'
      exit 0
      ;;
    *)
      echo "ERROR: Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

# ---------------------------------------------------------------------------
# Validate required arguments
# ---------------------------------------------------------------------------
if [[ -z "$ACR_NAME" || -z "$IMAGE_NAME" ]]; then
  echo "ERROR: --acr and --image are required." >&2
  echo "Run with --help for usage." >&2
  exit 1
fi

FULL_IMAGE="${ACR_NAME}.azurecr.io/${IMAGE_NAME}:${IMAGE_TAG}"

echo "=============================================="
echo "  MAF Hosted Agent — Build & Push to ACR"
echo "=============================================="
echo "  ACR      : ${ACR_NAME}.azurecr.io"
echo "  Image    : ${IMAGE_NAME}:${IMAGE_TAG}"
echo "  Full tag : ${FULL_IMAGE}"
echo "  Context  : ${BUILD_CONTEXT}"
echo "----------------------------------------------"

# ---------------------------------------------------------------------------
# Step 1: Authenticate with ACR
# ---------------------------------------------------------------------------
echo ""
echo "[1/4] Authenticating with ACR..."
az acr login --name "$ACR_NAME"

# ---------------------------------------------------------------------------
# Step 2: Build the image
# ---------------------------------------------------------------------------
echo ""
echo "[2/4] Building Docker image..."
docker build \
  --platform linux/amd64 \
  -t "${IMAGE_NAME}:${IMAGE_TAG}" \
  "$BUILD_CONTEXT"

# ---------------------------------------------------------------------------
# Step 3: Tag the image for ACR
# ---------------------------------------------------------------------------
echo ""
echo "[3/4] Tagging image for ACR..."
docker tag "${IMAGE_NAME}:${IMAGE_TAG}" "$FULL_IMAGE"

# ---------------------------------------------------------------------------
# Step 4: Push to ACR
# ---------------------------------------------------------------------------
echo ""
echo "[4/4] Pushing image to ACR..."
docker push "$FULL_IMAGE"

echo ""
echo "=============================================="
echo "  Done!  Image available at:"
echo "  ${FULL_IMAGE}"
echo "=============================================="
