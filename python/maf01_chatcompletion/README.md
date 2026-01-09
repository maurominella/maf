# Azure AI Agents sample - **ChatCompletion**

## Notes
⚠️ Important: Here are the libraries used here:
```
python==3.12.12

azure-ai-agents==1.2.0b5
azure-ai-projects==1.0.0b12
azure-ai-inference==1.0.0b9
agent-framework-core==1.0.0b251016
agent-framework-azure-ai==1.0.0b251016
agent-framework-devui==1.0.0b251016
```

## What this sample does


### UV Installation
- On Linux / MAC --> `curl -LsSf https://astral.sh/uv/install.sh | sh`
- On Windows --> `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

### Environment preparation
- *CD* into the folder
- Create the environment: `uv init . --python 3.12`.
- Add libraries (bad method): `uv add python-dotenv azure-ai-agents==1.2.0b5 azure-ai-projects==1.0.0b12 azure-ai-inference==1.0.0b9agent-framework-core==1.0.0b251016 agent-framework-azure-ai==1.0.0b251016 agent-framework-devui==1.0.0b251016 jupyter --prerelease=allow`.
- Add libraries (better method): `uv add $(cat requirements.txt) --prerelease=allow`.
- Syncrhonize to create the file structure: `uv sync --prerelease=allow`.
- Activate the environment:
  - on Linux/MC --> `source .venv/bin/activate`.
  - on Windows --> `.venv\Scripts\activate.ps1`.
- To deactivate --> `deactivate`.

## Reference docs
- [Azure AI Foundry Agents](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/azure-ai-foundry-agent?pivots=programming-language-python)
- This sample adopts [Azure OpenAI APIs next generation v1](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle?view=foundry-classic&tabs=python#api-evolution)