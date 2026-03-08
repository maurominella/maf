# Libraries and Variables
import asyncio
import os
from agent_framework.azure import AzureOpenAIChatClient
from agent_framework import Message
import sys
from dotenv import load_dotenv # package python-dotenv
config_path = "../../config"
sys.path.append(config_path)
from auth import acquire_bearer_token, StaticBearerTokenCredential

bearer_token = ""
openai_endpoint = ""
deployment_name = ""
openai_api_version = ""

messages_system = ""
messages_user = ""

def init():
    global openai_endpoint, deployment_name, openai_api_version, messages_system, messages_user
    openai_endpoint = os.getenv("AZURE_OPENAI_ENDPOINT") # AIF_STD_PROJECT_OPENAI_ENDPOINT DOES NOT WORK!!
    deployment_name = os.environ["MODEL_DEPLOYMENT_NAME"]
    openai_api_version = os.getenv("AZURE_OPENAI_API_VERSION")

    messages_system = "You are an AI assistant that helps people find information"
    messages_user = "Tell me five good names for my new pizzeria"

def authentication_and_environmentsetup():
    global bearer_token
    bearer_token, user = acquire_bearer_token(
        scope="https://cognitiveservices.azure.com/.default", # https://ai.azure.com/.default, https://cognitiveservices.azure.com/.default...
        auth_method="default") # default, cli, device

    if not load_dotenv(f"{config_path}/credentials_my.env"):
        print("Environment variables not loaded, cell execution stopped")
    else:
        print("Environment variables have been loaded ;-)")

    print("Bearer token:", bearer_token[:10], "...")
    print(f'User info: {user["name"]}, upn: {user["upn"]}')
    # user['raw_claims']


async def basic_example():
    # Create an agent using Azure OpenAI ChatCompletion
    cc = AzureOpenAIChatClient(
        credential = StaticBearerTokenCredential(bearer_token),
        endpoint = openai_endpoint.rstrip("/") + "/",
        deployment_name = deployment_name,
        api_version = openai_api_version,
    )

    agent = cc.as_agent() # python object that implements the Agent interface, so you can call agent.run() to get a response from the model

    response = await agent.run( # this is asyncrhonous
        [
            Message(role="system", contents=messages_system),
            Message(role="user", contents=messages_user),
        ]
    )
    print(response.text)    


def main():
    authentication_and_environmentsetup()
    init()
    asyncio.run(basic_example())


if __name__ == "__main__":
    main()