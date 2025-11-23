# Azure OpenAI Responses Agents sample

## UV Installation
- on Linux / MAC --> `curl -LsSf https://astral.sh/uv/install.sh | sh`
- on Windows --> `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

## Steps
- create the environment: `uv init maf02_responses --python 3.12`
- move into the folder that is automatically created: `cd maf02_responses`
- add libraries: `uv add agent-framework python-dotenv jupyter`
- syncrhonize to create the file structure: `uv sync`
- Activate the environment:
  - on Linux/MC --> `source .venv/bin/activate`
  - on Windows --> `.\.venv\Scripts\activate.ps1`
- to deactivate --> `deactivate`

## Reference docs
- [Azure OpenAI Responses Agents](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/azure-openai-responses-agent?pivots=programming-language-python)
- This sample adopts [Azure OpenAI APIs next generation v1](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle?view=foundry-classic&tabs=python#api-evolution)