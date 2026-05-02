#!/usr/bin/env bash
# build_and_push.sh — build a container image and push it to Azure Container Registry (ACR)
#
# Usage:
#   bash build_and_push.sh [OPTIONS]
#
# Options:
#   -r, --registry    ACR registry name (without .azurecr.io)  [required]
#   -p, --repository  Image repository / namespace             [default: hosted-agents]
#   -i, --image       Image name                               [required]
#   -t, --tag         Image tag                                [default: latest]
#   -c, --context     Docker build context directory           [default: .]
#   -f, --file        Path to Dockerfile                       [default: <context>/Dockerfile]
#   -h, --help        Show this help message and exit
#
# Example:
#   bash scripts/build_and_push.sh \
#     --registry   myfoundryacr \
#     --repository hosted-agents \
#     --image      ha01-echoagent \
#     --tag        latest \
#     --context    agents/ha01_echoagent
#
# Prerequisites:
#   - Docker installed and running
#   - Azure CLI installed (`az`)
#   - Logged in to Azure: `az login`
#
# No secrets are committed or embedded by this script.
# Pass secrets via environment variables or --env-file at runtime.

set -euo pipefail

# ── Defaults ────────────────────────────────────────────────────────────────
REGISTRY=""
REPOSITORY="hosted-agents"
IMAGE=""
TAG="latest"
CONTEXT="."
DOCKERFILE=""

# ── Argument parsing ─────────────────────────────────────────────────────────
usage() {
  cat <<'USAGE'
build_and_push.sh — Build a container image and push it to Azure Container Registry (ACR)

Usage:
  bash build_and_push.sh [OPTIONS]

Options:
  -r, --registry    ACR registry name (without .azurecr.io)  [required]
  -p, --repository  Image repository / namespace             [default: hosted-agents]
  -i, --image       Image name                               [required]
  -t, --tag         Image tag                                [default: latest]
  -c, --context     Docker build context directory           [default: .]
  -f, --file        Path to Dockerfile                       [default: <context>/Dockerfile]
  -h, --help        Show this help message and exit

Example:
  bash scripts/build_and_push.sh \
    --registry   myfoundryacr \
    --repository hosted-agents \
    --image      ha01-echoagent \
    --tag        latest \
    --context    agents/ha01_echoagent

Prerequisites:
  - Docker installed and running
  - Azure CLI installed (az)
  - Logged in to Azure: az login

No secrets are committed or embedded by this script.
Pass secrets via environment variables or --env-file at docker run time.
USAGE
  exit 0
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -r|--registry)    REGISTRY="$2";    shift 2 ;;
    -p|--repository)  REPOSITORY="$2";  shift 2 ;;
    -i|--image)       IMAGE="$2";       shift 2 ;;
    -t|--tag)         TAG="$2";         shift 2 ;;
    -c|--context)     CONTEXT="$2";     shift 2 ;;
    -f|--file)        DOCKERFILE="$2";  shift 2 ;;
    -h|--help)        usage ;;
    *) echo "Unknown option: $1" >&2; usage ;;
  esac
done

# ── Validation ───────────────────────────────────────────────────────────────
ERRORS=()
[[ -z "$REGISTRY" ]] && ERRORS+=("--registry is required")
[[ -z "$IMAGE"    ]] && ERRORS+=("--image is required")
if [[ ${#ERRORS[@]} -gt 0 ]]; then
  for err in "${ERRORS[@]}"; do echo "ERROR: $err" >&2; done
  echo "" >&2
  usage
fi

# ── Derived values ────────────────────────────────────────────────────────────
LOGIN_SERVER="${REGISTRY}.azurecr.io"
FULL_IMAGE="${LOGIN_SERVER}/${REPOSITORY}/${IMAGE}:${TAG}"
DOCKERFILE_ARG=""
if [[ -n "$DOCKERFILE" ]]; then
  DOCKERFILE_ARG="--file ${DOCKERFILE}"
fi

# ── Steps ─────────────────────────────────────────────────────────────────────
echo "============================================================"
echo " Microsoft Foundry Hosted Agent — Build & Push to ACR"
echo "============================================================"
echo "  Registry   : ${LOGIN_SERVER}"
echo "  Full image : ${FULL_IMAGE}"
echo "  Context    : ${CONTEXT}"
echo "============================================================"
echo ""

# 1. ACR login
echo "[1/3] Logging in to ACR: ${LOGIN_SERVER} ..."
az acr login --name "${REGISTRY}"
echo ""

# 2. Docker build
echo "[2/3] Building image: ${FULL_IMAGE} ..."
# shellcheck disable=SC2086
docker build ${DOCKERFILE_ARG} --tag "${FULL_IMAGE}" "${CONTEXT}"
echo ""

# 3. Docker push
echo "[3/3] Pushing image: ${FULL_IMAGE} ..."
docker push "${FULL_IMAGE}"
echo ""

echo "============================================================"
echo " Done! Image available at:"
echo "   ${FULL_IMAGE}"
echo "============================================================"
