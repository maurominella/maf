#region Common Libraries & Global variables
# Common Libraries
import os, sys
from dotenv import load_dotenv # requires python-dotenv
from IPython.display import Markdown, display
from agent_framework import ai_function

config_path = "../../../config" # explicit path to the config folder
sys.path.append(config_path)
from auth import acquire_bearer_token, StaticBearerTokenCredential
if not load_dotenv(f"{config_path}/credentials_my.env"):
    print("Environment variables not loaded, cell execution stopped")
else:
    print("Environment variables have been loaded ;-)")

# Global variables - recall to declare them as "global" in the functions where they are assigned
project_endpoint = "" # must be Foundry V1 project!
deployment_name = ""
agent_name = ""
credential = None
acr_endpoint = ""
acr_image = ""
#endregion

@ai_function
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
    global project_endpoint, deployment_name, credential, agent_name, acr_endpoint, acr_image
    
    project_endpoint = os.getenv("AIF_STD_PROJECT_ENDPOINT") # must be Foundry V1 project
    deployment_name = os.getenv("MODEL_DEPLOYMENT_NAME")
    agent_name = "hosted-agent-01"
    acr_endpoint = os.getenv("ACR_ENDPOINT")
    acr_image = "my-image:tag"
    credential = StaticBearerTokenCredential(bearer_token_azureai)    
    

def create_hosted_agent():
    # Initialize the client
    from azure.ai.projects import AIProjectClient
    from azure.ai.projects.models import HostedAgentDefinition, ProtocolVersionRecord, AgentProtocol

    project_client = AIProjectClient(
        endpoint=project_endpoint,
        credential=credential,
        allow_preview=True,
    )

    # Create the agent from a container image
    agent = project_client.agents.create_version(
        agent_name=agent_name,
        definition=HostedAgentDefinition(
            container_protocol_versions=[ProtocolVersionRecord(protocol=AgentProtocol.RESPONSES, version="v1")],
        cpu="1",
        memory="2Gi",
        image=f"{acr_endpoint}/{acr_image}",
        environment_variables={
            "AZURE_AI_PROJECT_ENDPOINT": project_endpoint,
            "MODEL_NAME": deployment_name,
            "CUSTOM_SETTING": "value"
        }    
        )    
    )

    return agent

def get_local_date_time(iana_timezone: str) -> str:
    """
    Get the current date and time for a given timezone.
    
    This is a LOCAL Python function that runs on the server - demonstrating how code-based agents
    can execute custom logic that prompt agents cannot access.
    
    Args:
        iana_timezone: The IANA timezone string (e.g., "America/Los_Angeles", "America/New_York", "Europe/London")
    
    Returns:
        The current date and time in the specified timezone.
    """

    from datetime import datetime
    from zoneinfo import ZoneInfo

    try:
        tz = ZoneInfo(iana_timezone)
        current_time = datetime.now(tz)
        return f"The current date and time in {iana_timezone} is {current_time.strftime('%A, %B %d, %Y at %I:%M %p %Z')}"
    except Exception as e:
        return f"Error: Unable to get time for timezone '{iana_timezone}'. {str(e)}"


def create_hosting_adapter_agent():
    # Create the agent with a local Python tool

    from agent_framework import ChatAgent
    from agent_framework.azure import AzureAIAgentClient

    hosting_adapter_agent = ChatAgent(
        chat_client=AzureAIAgentClient(
            project_endpoint=project_endpoint,
            model_deployment_name=deployment_name,
            credential=credential,
        ),
        instructions="You are a helpful assistant that can tell users the current date and time in any location. When a user asks about the time in a city or location, use the get_local_date_time tool with the appropriate IANA timezone string for that location.",
        tools=[get_local_date_time],
    )
    return hosting_adapter_agent

def main():
    authentication_and_environmentsetup()
    init()

    my_hosting_adapter_agent = create_hosting_adapter_agent()
    
    # my_hosted_agent = create_hosted_agent()    
    # print(f"Agent created: {my_hosted_agent.name} (id: {my_hosted_agent.id}, version: {my_hosted_agent.version})")
    

if __name__ == "__main__":
    main()