# Azure OpenAI Responses Agents sample

## UV Installation
- On Linux / MAC --> `curl -LsSf https://astral.sh/uv/install.sh | sh`
- On Windows --> `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

## Steps
- Create the environment: `uv init maf02_responses --python 3.12`.
- Move into the folder that is automatically created: `cd maf02_responses`.
- Add libraries: `uv add agent-framework python-dotenv jupyter`.
- Syncrhonize to create the file structure: `uv sync`.
- Activate the environment:
  - on Linux/MC --> `source .venv/bin/activate`.
  - on Windows --> `.\.venv\Scripts\activate.ps1`.
- To deactivate --> `deactivate`.

## What this sample does
This sample creates an agent with the Microsoft Agent Framework that relies on the Azure OpenAI Responses service.<br/>
It uses the direct endpoint -like `https://my-azure-openai-resource.openai.azure.com/` of an Azure OpenAI resource.<br/>
- First, it creates the client -`AzureOpenAIResponsesClient`- to the Azure OpenAI service that is responses-API enabled.
- Then, it uses the client to create the agent.
- Optionally, it can engage AssistantApi-like tools like *CodeInterpreter* and *FileSearch*.

## Reference docs
- [Azure OpenAI Responses Agents](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/azure-openai-responses-agent?pivots=programming-language-python)
- This sample adopts [Azure OpenAI APIs next generation v1](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle?view=foundry-classic&tabs=python#api-evolution)