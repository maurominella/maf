# Microsoft Foundry Hosted Agents — End-to-End Guide

> **Note:** All samples in this repository are provided to accelerate development. Review them carefully and test outputs in the context of your use case. AI responses may be inaccurate; enable human oversight in production workloads. See the [Agent Service transparency note](https://learn.microsoft.com/en-us/azure/ai-foundry/responsible-ai/agents/transparency-note) and the [Agent Framework FAQ](https://github.com/microsoft/agent-framework/blob/main/TRANSPARENCY_FAQ.md).

---

## Overview

**Microsoft Foundry Hosted Agents** are custom AI agents that you write in Python using the [Microsoft Agent Framework (MAF)](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview), containerize, push to Azure Container Registry (ACR), and deploy directly into a Microsoft Foundry project. Unlike *prompt agents* (which run on the Foundry platform without code), hosted agents run your own Python code inside a container that Foundry manages.

This guide covers the complete lifecycle:

```
Write code → Run locally → Containerize → Push to ACR → Deploy to Foundry → Test
```

---

## Repository Structure

```
python/hosted_agents/
├── README.md                  ← this file (end-to-end guide)
├── agents/
│   ├── ha01_echoagent/        ← Minimal custom agent (BaseAgent, no LLM)
│   └── ha02-azureopenaiagent/ ← Azure OpenAI agent with local Python tools
├── clients/                   ← Test clients (HTTP, OpenAI SDK, MAF SDK)
├── scripts/
│   ├── build_and_push.sh      ← Bash: build → tag → push to ACR
│   └── build_and_push.ps1     ← PowerShell: build → tag → push to ACR
└── starter_kits/              ← azd-based deployment workflow
```

Each agent folder contains:

| File | Purpose |
|---|---|
| `main.py` | Agent entry point |
| `Dockerfile` | Container image definition |
| `.dockerignore` | Files excluded from the image build |
| `.env.sample` | Environment variable template (commit this) |
| `.env` | Your actual secrets (never commit this) |
| `agent.yaml` | Agent manifest (name, protocols, env vars) |
| `requirements.txt` | Python dependencies |

---

## 1. Prerequisites

### Azure resources

| Resource | Notes |
|---|---|
| **Azure subscription** | Free trial at [azure.microsoft.com/free](https://azure.microsoft.com/free) |
| **Microsoft Foundry project** | Create one in [Azure AI Foundry portal](https://ai.azure.com) |
| **Azure Container Registry (ACR)** | Created automatically by the Foundry extension, or provision manually |
| **Azure OpenAI model deployment** | e.g. `gpt-4o` or `gpt-4.1-mini` — needed for ha02 |

### Local tooling

| Tool | Install |
|---|---|
| **Python 3.10+** | [python.org/downloads](https://www.python.org/downloads/) or `winget install Python.Python.3.12` |
| **Docker Desktop** | [docs.docker.com/get-started/get-docker](https://docs.docker.com/get-started/get-docker/) |
| **Azure CLI** | [learn.microsoft.com/cli/azure/install-azure-cli](https://learn.microsoft.com/cli/azure/install-azure-cli) |
| **VS Code** | [code.visualstudio.com](https://code.visualstudio.com) |
| **uv** (optional, for local venv) | `curl -LsSf https://astral.sh/uv/install.sh \| sh` (Linux/macOS) or `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 \| iex"` (Windows) |

### VS Code extensions

Install all of the following from the Extensions panel (`Ctrl+Shift+X`):

| Extension | ID |
|---|---|
| **Microsoft Foundry** | `ms-azuretools.vscode-azure-ai-foundry` |
| **Docker** | `ms-azuretools.vscode-docker` |
| **Python** | `ms-python.python` |
| **Azure CLI Tools** | `ms-vscode.azurecli` |
| **REST Client** (optional) | `humao.rest-client` |

### Azure CLI authentication

```bash
az login
az account show   # verify the correct subscription is active
```

If you have multiple subscriptions:

```bash
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"
```

---

## 2. Local Development in VS Code

### 2a. Clone and open the workspace

```bash
git clone https://github.com/maurominella/maf.git
cd maf/python/hosted_agents/agents/ha02-azureopenaiagent   # or ha01_echoagent
code .
```

### 2b. Create a virtual environment

**Using `uv` (recommended):**

```bash
uv init . --python 3.13
uv venv
source .venv/bin/activate          # Linux/macOS
# .\.venv\Scripts\activate.ps1    # Windows PowerShell
uv add --active $(cat requirements.txt) --prerelease=allow
uv sync --active --prerelease=allow
```

**Using `pip` (alternative):**

```bash
python -m venv .venv
source .venv/bin/activate          # Linux/macOS
# .\.venv\Scripts\Activate.ps1    # Windows PowerShell
pip install -r requirements.txt
```

Verify the installation:

```bash
python - << 'EOF'
import agent_framework
print("OK:", agent_framework)
EOF
```

### 2c. Configure environment variables

Copy the sample file and fill in your values:

```bash
cp .env.sample .env
```

Edit `.env` (never commit this file):

```bash
# ha01_echoagent
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=xxxx;IngestionEndpoint=https://...
AZURE_EXPERIMENTAL_ENABLE_GENAI_TRACING=true

# ha02-azureopenaiagent (add these too)
PROJECT_ENDPOINT=https://<foundryaccount>.services.ai.azure.com/api/projects/<projectname>
MODEL_DEPLOYMENT_NAME=gpt-4.1-mini
```

> `APPLICATIONINSIGHTS_CONNECTION_STRING` is optional for local development; Foundry injects it automatically in production.

### 2d. Run the agent locally

**Option 1 — Press F5 in VS Code (recommended)**

Open `main.py` and press **F5**. The VS Code launch configuration (`.vscode/launch.json`) starts the HTTP server on `http://localhost:8088` with the debugger attached and opens the AI Toolkit Agent Inspector for interactive testing.

**Option 2 — Terminal**

```bash
python main.py
```

Expected output:

```
INFO  Azure Monitor is not configured. No connection string found.
INFO  Agent server running on http://localhost:8088
```

### 2e. Quick smoke-test (local)

```bash
curl -sS -H "Content-Type: application/json" -X POST http://localhost:8088/responses \
  -d '{"input": "Hello!", "stream": false}'
```

Or use the `.http` files in `clients/` with the VS Code REST Client extension.

---

## 3. Containerization

### 3a. Dockerfile

Both sample agents include a `Dockerfile`. The recommended pattern for a MAF agent is:

```dockerfile
FROM python:3.13-slim

WORKDIR /app

# Copy source (secrets are excluded via .dockerignore)
COPY ./ .

RUN pip install --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Port is informational only; Foundry routes traffic internally
EXPOSE 8088

CMD ["python", "main.py"]
```

Key points:
- Use a **slim** base image to keep the layer small.
- **Never** add `ENV` instructions for secrets — they are baked into every image layer.
- The `EXPOSE` instruction is documentation only; Foundry does not require a specific external port.

### 3b. `.dockerignore`

Exclude secrets and development artifacts from every build context:

```
# Virtual environments
.venv/
venv/
env/

# Secrets — NEVER bake into the image
.env
.env.*
*.local

# Python build artifacts
__pycache__/
*.py[cod]
*.egg-info/
dist/
build/

# IDE / OS noise
.vscode/
.idea/
.DS_Store

# Foundry metadata
.foundry/

# Git
.git/
.gitignore
```

> Both `ha01_echoagent` and `ha02-azureopenaiagent` ship with a `.dockerignore` file following this pattern.

### 3c. Environment variables at runtime

Pass env vars at `docker run` time — never bake them into the image:

```bash
# Option A: individual flags
docker run -p 8089:8088 \
  -e PROJECT_ENDPOINT="https://<foundryaccount>.services.ai.azure.com/api/projects/<projectname>" \
  -e MODEL_DEPLOYMENT_NAME="gpt-4.1-mini" \
  <image-name>

# Option B: .env file (keep out of source control)
docker run -p 8089:8088 --env-file .env <image-name>
```

---

## 4. Build, Tag & Push to ACR

### 4a. One-time ACR setup

If you don't already have an ACR:

```bash
# Replace placeholders with your values
ACR_NAME="<YOUR_ACR_NAME>"           # e.g. myregistry (globally unique, alphanumeric only)
RESOURCE_GROUP="<YOUR_RESOURCE_GROUP>"

az acr create \
  --name "$ACR_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --sku Basic \
  --admin-enabled true
```

### 4b. Log in to ACR

```bash
az acr login --name "$ACR_NAME"
```

### 4c. Build, tag, and push (manual)

```bash
ACR_NAME="<YOUR_ACR_NAME>"
IMAGE_NAME="ha02-azureopenaiagent"
IMAGE_TAG="latest"                   # or a semver like "1.0.0"
AGENT_DIR="agents/ha02-azureopenaiagent"

# Build
docker build -t "${IMAGE_NAME}:${IMAGE_TAG}" "$AGENT_DIR"

# Tag for ACR
docker tag "${IMAGE_NAME}:${IMAGE_TAG}" \
  "${ACR_NAME}.azurecr.io/${IMAGE_NAME}:${IMAGE_TAG}"

# Push
docker push "${ACR_NAME}.azurecr.io/${IMAGE_NAME}:${IMAGE_TAG}"
```

### 4d. Use the helper scripts

The `scripts/` folder contains ready-to-use scripts that wrap the steps above:

**Bash (Linux / macOS / WSL):**

```bash
cd python/hosted_agents
chmod +x scripts/build_and_push.sh

scripts/build_and_push.sh \
  --acr     "<YOUR_ACR_NAME>" \
  --image   "ha02-azureopenaiagent" \
  --tag     "latest" \
  --context "agents/ha02-azureopenaiagent"
```

**PowerShell (Windows):**

```powershell
cd python/hosted_agents

.\scripts\build_and_push.ps1 `
  -AcrName   "<YOUR_ACR_NAME>" `
  -ImageName "ha02-azureopenaiagent" `
  -Tag       "latest" `
  -Context   "agents\ha02-azureopenaiagent"
```

Both scripts call `az acr login` automatically before pushing.

---

## 5. Deploy to Microsoft Foundry

### Option A — VS Code extension (recommended)

1. Open the agent folder in VS Code.
2. Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`).
3. Run **`Microsoft Foundry: Deploy Hosted Agent`**.
4. Follow the interactive prompts:
   - Select your Foundry project.
   - Confirm or generate the `Dockerfile`.
   - The extension builds the container, pushes it to the project's ACR, and creates the hosted agent.
5. The deployed agent appears under **Hosted Agents (Preview)** in the Foundry extension tree.

> If the capability host is not yet provisioned, the extension will guide you through enabling it.

### Option B — `azd` workflow (starter kit)

See [`starter_kits/README.md`](starter_kits/README.md) for the complete Azure Developer CLI (`azd`) workflow, which provisions infrastructure, builds the image, and deploys the agent in one command.

```bash
# From the starter kit project folder
azd provision   # create ACR, capability host, App Insights
azd deploy ha01-echoagent
```

### Option C — Azure CLI (manual)

After pushing the image to ACR (step 4), create the hosted agent via the Foundry REST API or Azure CLI:

```bash
FOUNDRY_PROJECT_ENDPOINT="https://<foundryaccount>.services.ai.azure.com/api/projects/<projectname>"
AGENT_NAME="ha02-azureopenaiagent"
IMAGE="${ACR_NAME}.azurecr.io/${IMAGE_NAME}:${IMAGE_TAG}"

az ai foundry agent create \
  --project-endpoint "$FOUNDRY_PROJECT_ENDPOINT" \
  --name "$AGENT_NAME" \
  --image "$IMAGE" \
  --env PROJECT_ENDPOINT="$FOUNDRY_PROJECT_ENDPOINT" \
  --env MODEL_DEPLOYMENT_NAME="gpt-4.1-mini"
```

> Refer to the [Foundry CLI reference](https://learn.microsoft.com/en-us/cli/azure/ai/foundry) for the latest flag names as the product evolves.

### 5a. Runtime configuration — environment variables

Supply env vars when creating or updating the hosted agent. The extension reads `.env` from the workspace root and includes every key/value pair automatically. For the CLI / REST path, pass them explicitly.

| Variable | Required | Description |
|---|---|---|
| `PROJECT_ENDPOINT` | ha02 only | `https://<foundryaccount>.services.ai.azure.com/api/projects/<project>` |
| `MODEL_DEPLOYMENT_NAME` | ha02 only | Model deployment name (e.g. `gpt-4.1-mini`) |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Optional | Auto-injected by Foundry when App Insights is linked |
| `AZURE_EXPERIMENTAL_ENABLE_GENAI_TRACING` | Optional | Set to `true` to enable GenAI tracing in App Insights |
| `ENABLE_SENSITIVE_DATA` | Optional | Set to `true` to capture prompts/completions in traces (default: `false`) |

> **Security reminder:** Never store secrets in `agent.yaml`, `Dockerfile`, or any committed file. Use Foundry's secret management or Azure Key Vault references for production workloads.

### 5b. Identity and permissions (Managed Identity)

Hosted agents that call Azure services (e.g. Azure OpenAI, Azure AI Foundry) authenticate using the **project's system-assigned managed identity**. Grant it the required roles:

```bash
# Get the managed identity principal ID of the Foundry project
PRINCIPAL_ID=$(az ai foundry project show \
  --name "<YOUR_PROJECT_NAME>" \
  --resource-group "<YOUR_RESOURCE_GROUP>" \
  --query identity.principalId -o tsv)

# Assign Azure AI User role on the project resource
az role assignment create \
  --assignee "$PRINCIPAL_ID" \
  --role "Azure AI User" \
  --scope "/subscriptions/<SUB_ID>/resourceGroups/<RG>/providers/Microsoft.MachineLearningServices/workspaces/<PROJECT>"
```

Or use the Azure portal:

1. Open the Foundry project → **Access control (IAM)**.
2. **Add role assignment** → **Azure AI User**.
3. **Assign access to**: *Managed identity* → select your project's identity.
4. **Review + assign**. Allow a few minutes for propagation.

---

## 6. Testing & Verification

### 6a. Test locally (before pushing)

```bash
# Build and run the container locally
docker build -t ha02-azureopenaiagent:local agents/ha02-azureopenaiagent
docker run -p 8089:8088 --env-file agents/ha02-azureopenaiagent/.env ha02-azureopenaiagent:local

# In another terminal
curl -sS -H "Content-Type: application/json" -X POST http://localhost:8089/responses \
  -d '{"input": "Find me hotels in Seattle for March 20-23 under $200/night", "stream": false}'
```

### 6b. Test the deployed agent in Foundry

**Using the Foundry portal playground:**

1. Open [ai.azure.com](https://ai.azure.com) → your project → **Hosted Agents (Preview)**.
2. Select the deployed agent and click **Try in playground**.
3. Send a test message and verify the response.

**Using the REST Client (`clients/01 - echo_agent.http`):**

Update the `@endpoint` variable at the top of the file to point to your deployed agent URL, then send the request.

**Using the Python client (`clients/04 - Invoke HostedAgent_responses.py`):**

```bash
cd clients
pip install -r requirements.txt
python "04 - Invoke HostedAgent_responses.py"
```

### 6c. Verify logs and traces

- **Foundry portal** → your agent → **Traces** → **Log stream** — shows `stderr` output from the container.
- **Application Insights** → **Transaction search** — shows detailed OpenTelemetry traces (requires `APPLICATIONINSIGHTS_CONNECTION_STRING`).

---

## 7. Troubleshooting

### Docker build fails

| Symptom | Likely cause | Fix |
|---|---|---|
| `COPY failed: file not found` | File listed in `COPY` doesn't exist in build context | Verify the file exists; check `.dockerignore` isn't excluding it |
| `pip install` errors | Network/registry issues or version conflicts | Add `--no-cache-dir`; pin package versions in `requirements.txt` |
| `python: can't open file 'main.py'` | `WORKDIR` path mismatch | Ensure `WORKDIR` and `CMD` paths align |

### ACR authentication errors

| Symptom | Fix |
|---|---|
| `unauthorized: authentication required` | Run `az acr login --name <ACR_NAME>` again; tokens expire |
| `Error response from daemon: Head "...": denied` | Your Azure identity lacks AcrPush role — grant it with `az role assignment create --role AcrPush` |
| `DOCKER_BUILDKIT` issues on older Docker | Set `DOCKER_BUILDKIT=0` or upgrade Docker Desktop |

### Image pull errors in Foundry

| Symptom | Fix |
|---|---|
| `ImagePullBackOff` | ACR admin credentials not linked; ensure Foundry project has pull access to the ACR |
| `ErrImagePull: 401` | Grant Foundry's managed identity **AcrPull** on the ACR: `az role assignment create --role AcrPull --assignee <MSI_PRINCIPAL_ID> --scope <ACR_RESOURCE_ID>` |

### Agent doesn't start in Foundry

| Symptom | Fix |
|---|---|
| Container exits immediately | Check **Log stream** for Python exceptions; missing env var is a common cause |
| `ModuleNotFoundError` | A package in `requirements.txt` was not installed; rebuild the image |
| Agent returns 500 errors | Check that `PROJECT_ENDPOINT` and `MODEL_DEPLOYMENT_NAME` are set correctly |
| Managed identity auth fails | Wait a few minutes after role assignment; re-deploy if still failing |

### OpenTelemetry / App Insights warnings

At startup you may see:

```
WARNING Overriding of current LoggerProvider is not allowed
WARNING Overriding of current TracerProvider is not allowed
```

These are **cosmetic only**. They appear because both `main.py` and `agent_framework` call `configure_azure_monitor()` independently. They do not affect runtime behaviour.

---

## Additional Resources

- [Microsoft Agent Framework overview](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Azure AI Foundry documentation](https://learn.microsoft.com/en-us/azure/ai-foundry/)
- [azure-ai-agentserver-agentframework on PyPI](https://pypi.org/project/azure-ai-agentserver-agentframework/)
- [Managed Identities for Azure resources](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/)
- [Agent Service transparency note](https://learn.microsoft.com/en-us/azure/ai-foundry/responsible-ai/agents/transparency-note)
- [`ha01_echoagent` README](agents/ha01_echoagent/README.md) — echo agent details & logging guide
- [`ha02-azureopenaiagent` README](agents/ha02-azureopenaiagent/README.md) — hotel search agent details
- [`starter_kits` README](starter_kits/README.md) — `azd`-based deployment workflow
- [`clients` README](clients/README.md) — test client usage
