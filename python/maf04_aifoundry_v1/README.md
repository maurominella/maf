# Azure AI Foundry Agents sample - **V1**

## Notes
⚠️ Important: This sample uses the V1 version of Microsoft Azure AI Foundry. For newer projects, consider using the latest version of Azure AI Foundry which may have different APIs and improved capabilities. Here are the libraries used here:
```
azure-ai-agents==1.2.0b5
azure-ai-projects==1.0.0b12
azure-ai-inference==1.0.0b9
agent-framework-core==1.0.0b251016
agent-framework-azure-ai==1.0.0b251016
agent-framework-devui==1.0.0b251016
```

## What this sample does
The code showcases two different approaches for creating AI agents that can execute Python code:

1. **Simple Method** (`maf_aifoundry_agent_creation_simple`) - Streamlined approach for quick testing
2. **Full Method** (`maf_aifoundry_agent_creation_full`) - Comprehensive approach with separated concerns

Both methods create agents capable of executing Python code via the `HostedCodeInterpreterTool`.

## Implementation Details

### 1. Simple Agent Creation (`maf_aifoundry_agent_creation_simple`)

This method provides a **streamlined two-call approach** ideal for quick tests and prototyping:

- **`AzureAIAgentClient`** - Creates the project client with credentials, endpoint, and model deployment
- **`project_client.create_agent`** - Creates the agent with tools and instructions

The same client is used throughout for both agent creation and chat operations, minimizing setup complexity.

```python
project_client = AzureAIAgentClient(
    async_credential=credential,
    project_endpoint=project_endpoint,
    model_deployment_name=model_deployment_name)

agent = project_client.create_agent(
    agent_name=agent_name,
    instructions=instructions,
    tools=[HostedCodeInterpreterTool()])
```

### 2. Full Agent Creation (`maf_aifoundry_agent_creation_full`)
This method provides a comprehensive approach with separated concerns:

- **`AIProjectClient`** - Establishes connection to the Azure AI Project
- **`project_client.agents.create_agent`** - Creates the underlying agent resource
- **`AzureAIAgentClient`** - Wraps the created agent for client operations
- **`ChatAgent`** - Configures the agent with tools and conversation store

This separation of concerns provides more control over the agent lifecycle and configuration
```python
project_client = AIProjectClient(
    credential=credential,
    endpoint=project_endpoint)

created_agent = await project_client.agents.create_agent(
    name=agent_name,
    model=model_deployment_name,
    instructions=instructions)

agent_client = AzureAIAgentClient(
    project_client=project_client,
    agent_id=created_agent.id)

agent = agent_framework.ChatAgent(
    chat_client=agent_client,
    tools=[HostedCodeInterpreterTool()],
    store=True)
```

### Key Features
- Agent Invocation
The maf_agent_invocation function supports both streaming and non-streaming responses:

- Streaming mode - Displays responses as they're generated in real-time
Standard mode - Waits for complete response before displaying
- Resource Cleanup
The code implements proper async resource cleanup for:
- Azure credentials (AzureCliCredential)
- Project clients
- Chat clients
This ensures no resource leaks and proper connection management.


## Usage
### Variables
Set up environment variables in `credentials_my.env`:
- `AIF_BAS_PROJECT_ENDPOINT` - Your Azure AI Foundry project endpoint
- `AZURE_OPENAI_CHAT_DEPLOYMENT_NAME` - Your Azure OpenAI deployment name

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