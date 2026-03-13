# Microsoft Agent Framework sample for comparison with Semantic Kernel

## What this sample does
It uses the **Microsoft Agent Framework** to create an agent that relies on the Azure OpenAI Responses service:
- First, it creates the client ***AzureOpenAIResponsesClient*** to the Azure OpenAI endpoint -like `https://my-azure-openai-resource.openai.azure.com/`- that is [responses-API enabled](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/how-to/responses?view=foundry-classic&tabs=python-key).
- Then, it uses the client to create the agent.
- Bonus demo: it engages *AssistantApi* tools like **Function Tools** and **CodeInterpreter**.

## Reference docs
- [Azure AI Foundry Agents](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/azure-ai-foundry-agent?pivots=programming-language-python)
- This sample adopts [Azure OpenAI APIs next generation v1](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle?view=foundry-classic&tabs=python#api-evolution)

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
- To deactivate: `deactivate`
- Create kernel for the jupyter notebook: ```python -m ipykernel install --name maf00_from_sk_to_maf --use```
- Test Python:
```
python - << 'EOF'
import agent_framework
print("OK:", agent_framework)
EOF
```
