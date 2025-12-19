# Azure Simplest Possible MAF Agent to be Published as Hosted Agent

## UV Installation
- On Linux / macOS: `curl -LsSf https://astral.sh/uv/install.sh | sh`
- On Windows: `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`

## Setup Steps
- **CD** into the folder
- Create the environment: `uv init . --python 3.13`
- Add libraries:
  - Automatically: `uv add $(cat requirements.txt) --prerelease=allow`
  - Manually: `uv add <package-name> --prerelease=allow`
- Synchronize to create the file structure: `uv sync --prerelease=allow`
- Activate the environment:
  - On Linux/macOS: `source .venv/bin/activate`
  - On Windows: `.\.venv\Scripts\activate.ps1`
- To deactivate: `deactivate`

## What This Sample Does
It uses the **Microsoft Agent Framework** to create an agent that relies on the Azure OpenAI Responses service:
1. Creates the client **AzureOpenAIResponsesClient** to connect to an Azure OpenAI endpoint (e.g., `https://my-azure-openai-resource.openai.azure.com/`) that is [responses-API enabled](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/how-to/responses?view=foundry-classic&tabs=python-key)
2. Uses the client to create the agent
3. Tests the agent with a simple query
4. Exposes the agent through an API using the `from_agent_framework` function from the **azure-ai-agentserver-agentframework** package

## Required Environment Variables

### For Azure OpenAI Responses API
These must be set in your `.env` file:
- `AZURE_OPENAI_ENDPOINT` - Your Azure OpenAI endpoint URL
- `AZURE_OPENAI_CHAT_DEPLOYMENT_NAME` - Your chat deployment name
- `AZURE_OPENAI_RESPONSES_API_VERSION` - API version (e.g., `2024-10-01-preview`)

### For Hosted Agent (AI Foundry V2)
These are required to run the hosted agent server locally:
- `AZURE_AI_PROJECT_ENDPOINT` - Your AI Foundry V2 project endpoint (format: `https://{foundry-name}.services.ai.azure.com/api/projects/{project-name}`)
- `AZURE_SUBSCRIPTION_ID` - Your Azure subscription ID
- `AZURE_RESOURCE_GROUP_NAME` - Your resource group name
- `AZURE_AI_PROJECT_NAME` - Your project name

### Optional Environment Variables
These are only needed when deploying to Azure AI Foundry V2 (automatically set by the platform):
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - For telemetry and monitoring in Azure (not needed for local development)
- `AGENT_PROJECT_RESOURCE_ID` - To identify the agent within an AI Foundry project (format: `/subscriptions/{sub-id}/resourceGroups/{rg}/providers/Microsoft.MachineLearningServices/workspaces/{project-name}/agents/{agent-name}`)

## Running the Application
1. Ensure all required environment variables are set in your `.env` file
2. Run: `python main.py` or `uv run main.py`
3. The agent will:
   - First test itself with a simple query
   - Then start listening on `http://0.0.0.0:8088`

## Testing the Hosted Agent
Use a REST client (e.g., VS Code REST extension) to test the endpoint:

```http
POST http://localhost:8088/responses
Content-Type: application/json

{   
   "messages": "Where is Seattle?"
}