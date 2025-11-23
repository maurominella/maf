# UV Installation
- on Linux / MAC --> curl -LsSf https://astral.sh/uv/install.sh | sh
- on Windows --> powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"

# Steps
- create the environment: `uv init mcp_server_getweather --python 3.12`
- move into the folder that is automatically created: `cd mcp_server_getweather`
- syncrhonize to create the file structure: `uv sync`
- Activate the environment:
  - on Linux/MC --> `source .venv/bin/activate`
  - on Windows --> `.\.venv\Scripts\activate.ps1`
- to deactivate --> `deactivate`