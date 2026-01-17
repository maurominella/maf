import os, asyncio
import agent_framework # tested agent-framework=1.0.0b251216
from agent_framework.azure import AzureOpenAIResponsesClient # same package as the previous line
from dotenv import load_dotenv  # tested python-dotenv=1.2.1
from azure.identity import AzureCliCredential # tested azure.identity=1.26.0b1


async def main_async():
    # Environment variables loading
    if not load_dotenv("./../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    else:
        print("Environment variables have been loaded ;-)")

    # First, create the Azure OpenAI Responses client
    openai_response_client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        api_version = os.getenv("AZURE_OPENAI_FOR_RESPONSES_API_VERSION"), # v1
    )

    # Then, create the agent
    maf_openai_response_agent = openai_response_client.create_agent(
        name="HelpfulAssistant",
        instructions="You are a helpful assistant that can write and execute Python code.", 
        tools=[agent_framework.HostedCodeInterpreterTool()],
    )

    result = await maf_openai_response_agent.run("Write a Python function that returns the Fibonacci sequence up to 10 and execute it.")
    print(result.text)        
    
    
if __name__ == "__main__":
    asyncio.run(main_async())