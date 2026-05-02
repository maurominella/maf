# Microsoft Foundry Hosted Agents — Python

This folder contains everything you need to develop, containerize, and deploy **Microsoft Foundry Hosted Agents** built with the **Microsoft Agent Framework** in Python.

---

## Table of Contents

1. [Overview](#overview)
2. [Repository Layout](#repository-layout)
3. [Prerequisites](#prerequisites)
4. [Local Development](#local-development)
5. [Containerization](#containerization)
6. [Push to Azure Container Registry (ACR)](#push-to-azure-container-registry-acr)
7. [Deploy to Microsoft Foundry](#deploy-to-microsoft-foundry)
8. [Automation Scripts](#automation-scripts)
9. [Troubleshooting](#troubleshooting)

---

## Overview

A **Microsoft Foundry Hosted Agent** is a custom AI agent written in Python using the **Microsoft Agent Framework** (MAF). The agent is packaged as a container image, pushed to **Azure Container Registry (ACR)**, and deployed to a **Microsoft Foundry project** where it is hosted as a managed service.

The typical end-to-end workflow is:

```
Write agent in Python (VS Code)
        │
        ▼
Run & test locally (python main.py)
        │
        ▼
Build container image (Docker)
        │
        ▼
Push image to ACR (az acr login + docker push)
        │
        ▼
Deploy as a Hosted Agent in Microsoft Foundry project
```

---

## Repository Layout

```
python/hosted_agents/
│
├── README.md                          ← you are here
│
├── agents/                            ← one folder per agent
│   ├── ha01_echoagent/                ← sample: simple echo agent (no LLM calls)
│   │   ├── main.py
│   │   ├── Dockerfile
│   │   ├── requirements.txt
│   │   ├── .env.example               ← copy to .env and fill in values
│   │   ├── agent.yaml                 ← Foundry manifest
│   │   └── README.md
│   │
│   └── ha02-azureopenaiagent/         ← sample: agent backed by Azure OpenAI
│       ├── main.py
│       ├── Dockerfile
│       ├── requirements.txt
│       ├── .env.example
│       ├── agent.yaml
│       └── README.md
│
├── clients/                           ← sample clients that call hosted agents
│   ├── README.md
│   └── *.py / *.http
│
├── starter_kits/                      ← azd-based starter kits
│   └── azd-ai-starter-basic/
│
└── scripts/                           ← helper scripts (build, tag, push)
    ├── build_and_push.sh              ← Bash (Linux / macOS / WSL)
    ├── build_and_push.ps1             ← PowerShell (Windows)
    └── generate_env.py                ← scaffold a .env.example file
```

> **Adding a new agent:** create a new folder under `agents/` following the same layout as the existing samples, then reference it in the top-level `agent.yaml` or deploy it independently.

---

## Prerequisites

| Tool | Minimum version | Install |
|------|----------------|---------|
| Python | 3.10+ | <https://www.python.org/downloads/> |
| Docker Desktop (or Docker Engine) | 24.x+ | <https://docs.docker.com/get-docker/> |
| Azure CLI (`az`) | 2.60+ | <https://learn.microsoft.com/cli/azure/install-azure-cli> |
| VS Code | latest | <https://code.visualstudio.com/> |
| VS Code extension: **Microsoft Foundry** | latest | Search `Microsoft Foundry` in the Extensions marketplace |
| `uv` (optional, recommended for fast venv management) | 0.4+ | `curl -LsSf https://astral.sh/uv/install.sh \| sh` |

**Azure resources required:**

- A **Microsoft Foundry** account and project.
- An **Azure Container Registry (ACR)** — one is automatically created when you provision a Foundry project, or you can bring your own.
- (Optional) An **Application Insights** resource for distributed tracing.

---

## Local Development

### 1. Clone the repo and navigate to an agent

```bash
git clone https://github.com/maurominella/maf.git
cd maf/python/hosted_agents/agents/ha01_echoagent   # or ha02-azureopenaiagent
```

### 2. Create and activate a virtual environment

**Using `uv` (recommended):**

```bash
uv venv
source .venv/bin/activate          # Linux / macOS
.\.venv\Scripts\activate.ps1       # Windows PowerShell
```

**Using plain `venv`:**

```bash
python -m venv .venv
source .venv/bin/activate          # Linux / macOS
.\.venv\Scripts\Activate.ps1       # Windows PowerShell
```

### 3. Install dependencies

```bash
pip install -r requirements.txt
```

Or with `uv`:

```bash
uv add --active $(cat requirements.txt) --prerelease=allow
uv sync --active --prerelease=allow
```

### 4. Configure environment variables

Copy the provided template and fill in your values:

```bash
cp .env.example .env
# then edit .env with your actual values
```

> **Never commit `.env` files.** They are listed in `.gitignore`.

Typical variables (exact set depends on the agent — see each agent's `README.md`):

```dotenv
# Microsoft Foundry project endpoint
PROJECT_ENDPOINT=https://<foundry-account>.services.ai.azure.com/api/projects/<project-name>

# Chat model deployment name in Foundry
MODEL_DEPLOYMENT_NAME=gpt-4o

# (Optional) Application Insights
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=xxxx;IngestionEndpoint=https://...
```

### 5. Run the agent locally

```bash
python main.py
```

The agent starts an HTTP server on `http://localhost:8088` by default.

**Quick smoke test (bash):**

```bash
curl -sS -X POST http://localhost:8088/responses \
  -H "Content-Type: application/json" \
  -d '{"input": "Hello!", "stream": false}'
```

**Quick smoke test (PowerShell):**

```powershell
$body = @{ input = "Hello!"; stream = $false } | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:8088/responses -Method Post `
  -Body $body -ContentType "application/json"
```

### 6. Run with VS Code (recommended for debugging)

1. Open the agent folder in VS Code.
2. Press **F5** — VS Code uses the `.vscode/launch.json` included in the agent (if present) to start the agent and open the AI Toolkit Agent Inspector.
3. Set breakpoints and inspect the workflow interactively.

---

## Containerization

Each agent has its own `Dockerfile`. Build the image from the agent folder:

```bash
# From the agent directory, e.g. agents/ha01_echoagent
docker build -t <image-name>:<tag> .
```

**Example:**

```bash
docker build -t ha01-echoagent:latest .
```

**Run the container locally to verify it works:**

```bash
docker run --rm -p 8089:8088 \
  --env-file .env \
  ha01-echoagent:latest
```

Then test with the same `curl` or PowerShell commands shown in the previous section (use port `8089`).

> The container exposes port `8088` internally (see each `Dockerfile`). Map it to any external port you like with `-p <external>:8088`.

**Clean up local Docker resources (optional):**

```bash
docker rm -f $(docker ps -aq)   # remove all containers
docker rmi -f $(docker images -aq)  # remove all images
```

---

## Push to Azure Container Registry (ACR)

### 1. Identify your ACR

```bash
# List ACRs in your subscription
az acr list --output table
```

Your Foundry-provisioned ACR is typically named after the project (e.g. `myfoundryacr`). Note its **login server** (`<registry>.azurecr.io`).

### 2. Log in to ACR

```bash
az acr login --name <registry>
```

> Requires `az login` to have been run first. Use `az account show` to verify your active subscription.

### 3. Tag the image

```bash
docker tag <image-name>:<tag> <registry>.azurecr.io/<repository>/<image-name>:<tag>
```

**Example:**

```bash
docker tag ha01-echoagent:latest myfoundryacr.azurecr.io/hosted-agents/ha01-echoagent:latest
```

### 4. Push the image

```bash
docker push <registry>.azurecr.io/<repository>/<image-name>:<tag>
```

**Example:**

```bash
docker push myfoundryacr.azurecr.io/hosted-agents/ha01-echoagent:latest
```

> See [Automation Scripts](#automation-scripts) for a single-command alternative.

---

## Deploy to Microsoft Foundry

### Option A — VS Code extension (recommended)

1. Open the agent folder in VS Code.
2. Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`).
3. Run: **`Microsoft Foundry: Deploy Hosted Agent`**.
4. Follow the interactive prompts. The extension will:
   - Use or create an ACR for the target Foundry project.
   - Build and push the container image.
   - Read environment variables from `.env` (if present at the workspace root) and inject them into the hosted agent configuration.
   - Create the agent version in your Foundry project.
   - Start the agent container on the project's capability host (provisioning one if needed).
5. After deployment, the agent appears under **Hosted Agents (Preview)** in the VS Code extension tree. Click it to open the integrated playground.

### Option B — Azure Developer CLI (`azd`)

For agents scaffolded with the starter kit:

```bash
cd starter_kits/azd-ai-starter-basic

# Authenticate
azd auth login

# Set required variables
azd env set PROJECT_ENDPOINT  "https://<account>.services.ai.azure.com/api/projects/<project>"
azd env set MODEL_DEPLOYMENT_NAME "gpt-4o"

# Provision Azure resources (first-time only)
azd provision

# Build image, push to ACR, and deploy agent
azd deploy ha01-echoagent
```

See [`starter_kits/azd-ai-starter-basic/README.md`](starter_kits/azd-ai-starter-basic/README.md) for the full walkthrough.

### Managed Identity (required for remote execution)

When the agent runs inside Foundry, it authenticates to Azure services using the **project's Managed Identity** (MSI). You must grant it the **Azure AI User** role:

1. Azure Portal → your Foundry project → **Access control (IAM)**.
2. **Add role assignment** → search for **Azure AI User** → **Next**.
3. **Assign access to**: Managed identity → **Select members** → choose the managed identity of your Foundry project.
4. **Review + assign**. Allow a few minutes for the assignment to propagate.

---

## Automation Scripts

The `scripts/` folder contains helper scripts to automate the build-tag-push workflow.

### Bash (Linux / macOS / WSL) — `scripts/build_and_push.sh`

```bash
# Usage
bash scripts/build_and_push.sh \
  --registry   myfoundryacr \
  --repository hosted-agents \
  --image      ha01-echoagent \
  --tag        latest \
  --context    agents/ha01_echoagent
```

### PowerShell (Windows) — `scripts/build_and_push.ps1`

```powershell
# Usage
.\scripts\build_and_push.ps1 `
  -Registry   myfoundryacr `
  -Repository hosted-agents `
  -Image      ha01-echoagent `
  -Tag        latest `
  -Context    agents\ha01_echoagent
```

### Generate a `.env.example` — `scripts/generate_env.py`

```bash
python scripts/generate_env.py --agent ha01_echoagent
# Creates (or updates) agents/ha01_echoagent/.env.example
```

Run any script with `--help` / `-h` for full usage details.

---

## Troubleshooting

### Authentication errors (`az login` / ACR)

| Symptom | Fix |
|---------|-----|
| `az acr login` fails with "unauthorized" | Run `az login` first; verify the right subscription with `az account show`. |
| Docker push returns 403 | Token may have expired — run `az acr login --name <registry>` again. |
| Container can't reach Foundry endpoint | Ensure the MSI role assignment has propagated (wait ~5 min after IAM change). |

### Container port issues

| Symptom | Fix |
|---------|-----|
| `curl` returns "Connection refused" | Check `docker ps` — ensure the container is running. Verify the port mapping matches (`-p <host>:8088`). |
| Port 8088 already in use | Choose a different host port: `-p 9000:8088`. |

### Health checks

The agent server exposes a health endpoint:

```bash
curl http://localhost:8088/health
```

Expected response: `{"status": "ok"}` (or similar). If the agent returns a non-200 status, check the container logs:

```bash
docker logs <container-id>
```

### Environment variable issues

- Ensure `.env` is in the **same directory** where you run `python main.py` (or pass `--env-file .env` to `docker run`).
- If using VS Code, set the variables in your shell profile or VS Code's `launch.json` `env` section.
- Variables injected by Foundry at runtime override any `.env` file values.

### Python version

The Microsoft Agent Framework requires **Python 3.10 or higher**. Verify:

```bash
python --version
```

---

## Additional Resources

- [Microsoft Agent Framework — overview](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Azure AI AgentServer SDK (PyPI)](https://pypi.org/project/azure-ai-agentserver-agentframework/)
- [Microsoft Foundry documentation](https://learn.microsoft.com/en-us/azure/ai-foundry/)
- [Managed Identities for Azure Resources](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/)
- [Azure Container Registry documentation](https://learn.microsoft.com/en-us/azure/container-registry/)
- [Agent Service Transparency Note](https://learn.microsoft.com/en-us/azure/ai-foundry/responsible-ai/agents/transparency-note)
