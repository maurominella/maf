# Common Libraries
import os, sys
from dotenv import load_dotenv # requires python-dotenv
from IPython.display import Markdown, display

config_path = "../../../config" # explicit path to the config folder
sys.path.append(config_path)
from auth import acquire_bearer_token, StaticBearerTokenCredential
if not load_dotenv(f"{config_path}/credentials_my.env"):
    print("Environment variables not loaded, cell execution stopped")
else:
    print("Environment variables have been loaded ;-)")

# Global libraries - recall to declare them as "global" in the functions where they are assigned
project_endpoint = "" # must be Foundry V1 project!
deployment_name = ""
bearer_token_cognitiveservices = ""
bearer_token_cognitiveservices = ""
agent_name = ""


def authentication_and_environmentsetup():
    global bearer_token_cognitiveservices, bearer_token_azureai
    bearer_token_cognitiveservices, user_cognitiveservices = acquire_bearer_token(
        scope="https://cognitiveservices.azure.com/.default", # https://ai.azure.com/.default, https://cognitiveservices.azure.com/.default...
        auth_method="default") # default, cli, device

    bearer_token_azureai, _ = acquire_bearer_token(
        scope="https://ai.azure.com/.default", # https://ai.azure.com/.default, https://cognitiveservices.azure.com/.default...
        auth_method="default") # default, cli, device

    print("Bearer token Cognitive Services:", bearer_token_cognitiveservices[:10], "...")
    print("Bearer token Azure AI:", bearer_token_azureai[:10], "...")
    print(f'User info Cognitive Services: {user_cognitiveservices["name"]}, upn: {user_cognitiveservices["upn"]}')
    # bearer_token_cognitiveservices['raw_claims']


def init():
    global project_endpoint, deployment_name, agent_name
    
    project_endpoint = os.getenv("AIF_STD_PROJECT_ENDPOINT") # must be Foundry V1 project
    deployment_name = os.getenv("MODEL_DEPLOYMENT_NAME")
    agent_name = "my-hosted-agent-01" # choose a unique name for your agent


def create_agent():
    from azure.ai.projects import AIProjectClient
    from azure.ai.projects.models import HostedAgentDefinition, ProtocolVersionRecord, AgentProtocol
    # Initialize the client
    client = AIProjectClient(
        endpoint=project_endpoint,
        credential=StaticBearerTokenCredential(bearer_token_azureai),
        allow_preview=True,
    )

    # Create the agent from a container image
    agent = client.agents.create_version(
        agent_name=agent_name,
        definition=HostedAgentDefinition(
            container_protocol_versions=[ProtocolVersionRecord(protocol=AgentProtocol.RESPONSES, version="v1")],
            cpu="1",
            memory="2Gi",
            image="your-registry.azurecr.io/your-image:tag",
            environment_variables={
                "AZURE_AI_PROJECT_ENDPOINT": project_endpoint,
                "MODEL_NAME": deployment_name,
                "CUSTOM_SETTING": "value"
            }
        )
    )

    return agent


def main():
    print("Hello from hosted-agents!")
    authentication_and_environmentsetup()
    init()
    agent = create_agent()
    print(f"Agent created: {agent.name} (id: {agent.id}, version: {agent.version})")


if __name__ == "__main__":
    main()
