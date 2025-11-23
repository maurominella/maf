# UV Installation
- on Linux / MAC --> `curl -LsSf https://astral.sh/uv/install.sh | sh`
- on Windows --> `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

# Steps
- create the environment: `uv init maf01_chatcompletion --python 3.12`
- move into the folder that is automatically created: `cd maf01_chatcompletion`
- syncrhonize to create the file structure: `uv sync`
- Activate the environment:
  - on Linux/MC --> `source .venv/bin/activate`
  - on Windows --> `.\.venv\Scripts\activate.ps1`
- to deactivate --> `deactivate`