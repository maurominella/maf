# Azure OpenAI Responses Agents sample

## UV Installation
- On Linux / MAC --> `curl -LsSf https://astral.sh/uv/install.sh | sh`
- On Windows --> `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

## Steps
- ***CD*** into the folder
- Create the environment: `uv init . --python 3.13`.
- Add libraries: 
  - automatically: `uv add $(cat requirements.txt) --prerelease=allow`.
  - manually: `uv add agent-framework azure-ai-projects>=2.0.0b1 azure-identity python-dotenv --prerelease=allow`.
- Syncrhonize to create the file structure: `uv sync --prerelease=allow`.
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
- [Azure OpenAI Responses Agents](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/azure-openai-responses-agent?pivots=programming-language-python)
- This sample adopts [Azure OpenAI APIs next generation v1](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle?view=foundry-classic&tabs=python#api-evolution)