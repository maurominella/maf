import os
import asyncio
import agent_framework
from dotenv import load_dotenv  # requires python-dotenv
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential, DefaultAzureCredential, get_bearer_token_provider
from openai import OpenAI

def maf_responsesai_agent_creation() -> agent_framework.ChatAgent:
    # Create an agent using Azure OpenAI Responses
    openairesponses_client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        api_version = os.getenv("AZURE_OPENAI_RESPONSES_API_VERSION"), # v1
    )

    openairesponses_agent = openairesponses_client.create_agent(
        name="HelpfulAssistant",
        instructions="You are a helpful assistant that can write and execute Python code.", 
        tools=[agent_framework.HostedCodeInterpreterTool()],
    )

    return openairesponses_agent


async def maf_agent_invokation(agent: agent_framework.ChatAgent, question: str, streaming: bool=False) -> str:
    response = ""
    print("Agent: ", end="", flush=True)
    if streaming:
        async for chunk in agent.run_stream(question):
            if chunk.text:
                print(chunk.text, end="", flush=True)
                response += chunk.text
    else:
        result = await agent.run(question)
        print(result.text)
        response = result.text
    return response
        

def main():
    # Environment variables loading
    if not load_dotenv("./../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    else:
        print("Environment variables have been loaded ;-)")

    agent = maf_responsesai_agent_creation()

    response1 = asyncio.run(maf_agent_invokation(agent, "Write a Python function that returns the Fibonacci sequence up to n and execute it with n=10.", streaming=False))
    response2 = asyncio.run(maf_agent_invokation(agent, "Tell me a story about a haunted house.", streaming=True))
    
    print("\n\n" + "*"*80 + " RESPONSE #1")
    print(response1)

    print("\n\n" + "*"*80 + " RESPONSE #2")
    print(response2) 
    
if __name__ == "__main__":
    main()