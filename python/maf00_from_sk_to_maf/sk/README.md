# Semantic Kernel sample for comparison with Microsoft Agent Framework

## UV Installation
- On Linux / MAC --> `curl -LsSf https://astral.sh/uv/install.sh | sh`
- On Windows --> `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

## Steps
- Create the environment: `uv init sk --python 3.12`.
- Move into the folder that is automatically created: `cd sk`.
- Add libraries: `uv add semantic-kernel python-dotenv jupyter`.
- Syncrhonize to create the file structure: `uv sync`.
- Activate the environment:
  - on Linux/MC --> `source .venv/bin/activate`.
  - on Windows --> `.\.venv\Scripts\activate.ps1`.
- To deactivate --> `deactivate`.

## What this sample does
It uses the **Microsoft Agent Framework** to create an agent that relies on the Azure OpenAI Responses service:
- First, it creates the client ***AzureOpenAIResponsesClient*** to the Azure OpenAI endpoint -like `https://my-azure-openai-resource.openai.azure.com/`- that is [responses-API enabled](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/how-to/responses?view=foundry-classic&tabs=python-key).
- Then, it uses the client to create the agent.
- Bonus demo: it engages *AssistantApi* tools like **Function Tools** and **CodeInterpreter**.

## Reference docs
- [Azure AI Foundry Agents](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/azure-ai-foundry-agent?pivots=programming-language-python)
- This sample adopts [Azure OpenAI APIs next generation v1](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle?view=foundry-classic&tabs=python#api-evolution)
