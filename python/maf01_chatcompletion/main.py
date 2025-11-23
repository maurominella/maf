import asyncio
import os
from agent_framework.azure import AzureOpenAIChatClient
from azure.identity import AzureCliCredential
from dotenv import load_dotenv  # requires python-dotenv

async def basic_example():
    # Create an agent using Azure OpenAI ChatCompletion
    agent = AzureOpenAIChatClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        deployment_name=os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        api_version=os.getenv("AZURE_OPENAI_API_VERSION"),
    ).create_agent(
        name="HelpfulAssistant",
        instructions="You are good at telling jokes.",
    )
    result = await agent.run("Tell me a joke about a pirate.")
    print(result.text)


def main():
    # Environment variables loading
    if not load_dotenv("./../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    else:
        print("Environment variables have been loaded ;-)")

    # os.environ["AZURE_OPENAI_ENDPOINT"] = os.getenv("AZURE_OPENAI_ENDPOINT")
    # os.environ["AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"] = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME")
    # os.environ["AZURE_OPENAI_API_VERSION"] = os.getenv("AZURE_OPENAI_API_VERSION")
    # os.environ["AZURE_OPENAI_API_KEY"] = os.getenv("AZURE_OPENAI_API_KEY")


    print("Hello from maf01-chatcompletion!")
    asyncio.run(basic_example())

if __name__ == "__main__":
    main()