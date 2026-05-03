# Microsoft Agent Framework

MAF Experiments.

This repository is a hands-on **Microsoft Agent Framework (MAF) experimentation and accelerator lab** for building, hosting, and integrating AI agents across **.NET, Python, Azure OpenAI, Azure AI Foundry, and Microsoft Foundry Hosted Agents**.

It helps developers move from isolated AI model calls to real agent-based applications. It provides practical samples showing how to wrap OpenAI and Azure OpenAI clients as MAF agents, add tools and workflows, compare MAF with Semantic Kernel patterns, host agents behind HTTP endpoints, containerize Python agents, and deploy them into Microsoft Foundry as managed hosted agents.

## What this repo helps you do

- Build agents with the **Microsoft Agent Framework**
- Integrate **OpenAI**, **Azure OpenAI**, and **Azure AI Foundry**
- Use tools and functions inside agents
- Expose agents through ASP.NET or AgentServer-style APIs
- Package Python agents into Docker containers
- Push and deploy agents to **Azure Container Registry** and **Microsoft Foundry**
- Invoke hosted agents from Python, HTTP clients, OpenAI SDK-style flows, or MAF SDK-style flows

The repository is a progression of examples. The C# side walks through native OpenAI clients, MAF adapters, Azure OpenAI with tools, Foundry agents, ASP.NET hosting, Aspire hosting, and fuller demos. The Python side adds quick tests, Semantic Kernel-to-MAF comparisons, mixed Foundry workflows, hosted agent samples, clients, deployment scripts, Dockerfiles, and starter kits.

In short, this is a practical playground and accelerator for building Microsoft Agent Framework agents in C# and Python, integrating them with OpenAI and Azure AI Foundry, and deploying them as hosted agents in Microsoft Foundry.

Please refer to the README.md files in each subfolder for focused setup and usage details.


## Quick Start

### UV Installation
- On Linux / macOS: `curl -LsSf https://astral.sh/uv/install.sh | sh`
- On Windows: `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

### Setup Steps
```python
# 1. **MKDIR** the new folder and and **CD** into it

# 2 Create the environment
uv init . --python 3.13

# 3. Create the local virtual environment
uv venv

# 4. Activate the environment:
source .venv/bin/activate # on Linux/macOS
.\.venv\Scripts\activate.ps1 # on Windows

# 5. Add libraries (it's KEY to use `--active`):
uv add --active $(cat requirements.txt) --prerelease=allow # Automatically
uv add --active <package-name> --prerelease=allow # Manually

# 6. Check that the packges are installed
uv pip list

# 7. Synchronize to create the file structure (not needed in normal situations, just with pre-existing pyproject.toml
uv sync --active --prerelease=allow

# 8. List jupyter kernels
jupyter kernelspec list

# 9. Delete a jupyter kernel
jupyter kernelspec uninstall responses

# 10. Create kernel for the jupyter notebook
python -m ipykernel install --name responses --use

# 11. To deactivate
deactivate
```

**Test it:**
```bash
curl -sS -X POST http://localhost:8088/responses \
  -H "Content-Type: application/json" \
  -d '{"input": "Hello!", "stream": false}'
```

```python
python - << 'EOF'
import agent_framework
print("OK:", agent_framework)
EOF
```
