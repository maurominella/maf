#region Common Libraries & Global variables
# Common Libraries
import os, sys
import asyncio
from dotenv import load_dotenv # requires python-dotenv
from agent_framework import Agent
from azure.ai.agentserver.agentframework import from_agent_framework


# Global variables - recall to declare them as "global" in the functions where they are assigned
config_path = "../../../config" # explicit path to the config folder
sys.path.append(config_path)
from auth import acquire_bearer_token, StaticBearerTokenCredential
if not load_dotenv(f"{config_path}/credentials_my.env"):
    print("Environment variables not loaded, cell execution stopped")
else:
    print("Environment variables have been loaded ;-)")

project_endpoint = "" # must be Foundry V1 project!
deployment_name = ""
agent_name = ""
agent_instructions = ""
credential = None
query = "Where is Seattle? What time is it there right now?"
#endregion

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
    global project_endpoint, deployment_name, credential, agent_name, agent_instructions
    
    project_endpoint = os.getenv("AIF_STD_PROJECT_ENDPOINT") # must be Foundry V2 project
    deployment_name = os.getenv("MODEL_DEPLOYMENT_NAME")
    agent_name = "local-hosting-agent-01"
    agent_instructions = "You are a helpful assistant that can tell users the current date and time in any location. When a user asks about the time in a city or location, use the get_local_date_time tool with the appropriate IANA timezone string for that location."
    credential = StaticBearerTokenCredential(bearer_token_azureai)    
   

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


async def create_maf_agent_async():
    from agent_framework.azure import AzureOpenAIResponsesClient

    azureopenai_client = AzureOpenAIResponsesClient( # this call is async
        deployment_name=deployment_name,
        project_endpoint=project_endpoint,
        credential=credential)
    
    # option 1 - manually creating the agent by passing the client, instructions, and tools. This gives more control and flexibility over the agent configuration, but requires more code and understanding of the underlying components.
    agent = Agent(
        client=azureopenai_client,
        instructions=agent_instructions,
        tools=[get_local_date_time]  # Our random destination tool function
    )

    # option 2 - using the as_agent method to create the agent directly from the client, which abstracts away some of the details and is more concise. This is also an async call.
    """
    agent = azureopenai_client.as_agent(
        name=agent_name,
        instructions=agent_instructions,
        tools=[get_local_date_time])
    """

    return agent


async def main():
    authentication_and_environmentsetup()
    init()

    agent1 = await create_maf_agent_async() # async call to create agent using AzureOpenAIResponsesClient.as_agent() method
    response1 = await agent1.run(query)
    print(f"# Agent1: {response1.text}")
    
    return agent1


if __name__ == "__main__":
    agent = asyncio.run(main())

    # here we convert the agent_framework Agent to an Azure AI AgentServer agent and run it, which will start a local server 
    # hosting the agent and allow us to interact with it via API calls or a playground UI. 
    # Note that the agent must be created with tools that are compatible with the Azure AI AgentServer environment for this to work properly.
    from_agent_framework(agent).run()

    print("Program executed successfully")