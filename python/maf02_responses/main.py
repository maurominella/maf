import os
import asyncio
from dotenv import load_dotenv  # requires python-dotenv
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential, DefaultAzureCredential, get_bearer_token_provider
from openai import OpenAI

# Azure OpenAI APIs next generation v1 example
async def basic_example_v1():
    # Create an agent using Azure OpenAI Responses
    token_provider = get_bearer_token_provider(
        DefaultAzureCredential(), "https://cognitiveservices.azure.com/.default"
    )
    client = OpenAI(  
    base_url = f"{os.getenv("AZURE_OPENAI_ENDPOINT")}openai/v1/",  
    api_key = token_provider  
    )
    response = client.responses.create(
        model="gpt-4.1",
        input= "This is a test" 
    )
    print(response.output_text) # response.model_dump_json(indent=2))


async def basic_example():
    # Create an agent using Azure OpenAI Responses
    agent = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        api_version = os.getenv("AZURE_OPENAI_RESPONSES_API_VERSION"),
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

    print("Hello from maf02-responses!")
    asyncio.run(basic_example_v1())

if __name__ == "__main__":
    main()