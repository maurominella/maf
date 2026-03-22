# HOSTED AGENTS

## UV Installation
- On Linux / macOS: `curl -LsSf https://astral.sh/uv/install.sh | sh`
- On Windows: `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

## Setup Steps
- **CD** into the folder
- Create the environment: `uv init . --python 3.13`
- Create the local virtual environment: `uv venv`
- Activate the environment:
  - On Linux/macOS: `source .venv/bin/activate`
  - On Windows: `.\.venv\Scripts\activate.ps1`
- Add libraries (it's KEY to use `--active`):
  - Automatically: `uv add --active $(cat requirements.txt) --prerelease=allow`
  - Manually: `uv add --active <package-name> --prerelease=allow`
- Check that the packges are installed: `uv pip list`
- Synchronize to create the file structure: `uv sync --active --prerelease=allow`
- List jupyter kernels: `jupyter kernelspec list`
- Delete a jupyter kernel: `jupyter kernelspec uninstall maf01`
- Create kernel for the jupyter notebook: ```python -m ipykernel install --name maf01 --use```
- Test Python:
```
python - << 'EOF'
import agent_framework
print("OK:", agent_framework)
EOF
```
- To deactivate: `deactivate`

## Requirements
Lists the exact versions from your uv pip freeze output. The agent-framework-*==1.0.0rc3 entries are explicitly listed so pip knows which version you want — the comment explains these must override what azure-ai-agentserver-agentframework would normally pull.


## Dockerfile
```
# Step 1: install agentserver + all other deps (this pulls in old agent-framework-* as transitive deps)
RUN pip install --no-cache-dir --pre \
        azure-ai-agentserver-agentframework==1.0.0b17 ...

# Step 2: upgrade agent-framework-* to the rc3 versions you need
    pip install --no-cache-dir --pre \
        "agent-framework-azure-ai==1.0.0rc3" \
        "agent-framework-core==1.0.0rc3"
```
- --pre is required for all installs because all packages are pre-release (b17, rc3, etc.)
- CMD ["python", "agent.py"] uses the fixed agent.py as entrypoint instead of main.py
- Port 8088 is exposed to match your local server

### Building and running locally to test before deploying:
- note that we're mapping the *internal* port 8088 to the *external* port 8089
```
cd python/hosted_agents/hostedagent01_echoagent
docker build -t hostedagent02 .
docker run -p 8089:8088 hostedagent01_echoagent
```