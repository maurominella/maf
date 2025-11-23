import os
import asyncio
from dotenv import load_dotenv  # requires python-dotenv
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential, DefaultAzureCredential, get_bearer_token_provider
from openai import OpenAI

async def basic_agent():
    # Create an agent using Azure OpenAI Responses
    openairesponses_client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        api_version = os.getenv("AZURE_OPENAI_RESPONSES_API_VERSION"), # v1
    )

    openai_agent = openairesponses_client.create_agent(
        name="HelpfulAssistant",
        instructions="You are good at telling jokes.",
    )

    result = await openai_agent.run("Tell me a joke about a pirate.")
    
    print(result.text)


def main():
    # Environment variables loading
    if not load_dotenv("./../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    else:
        print("Environment variables have been loaded ;-)")

    print("Hello from maf02-responses!")
    asyncio.run(basic_agent())

if __name__ == "__main__":
    main()