import os
import agent_framework
import asyncio
from dotenv import load_dotenv  # requires python-dotenv
from azure.identity.aio import AzureCliCredential
from agent_framework.azure import AzureAIAgentClient
from agent_framework import HostedCodeInterpreterTool

def maf_aifoundry_agent_creation() -> agent_framework.ChatAgent:
    """Create an agent using Azure OpenAI Responses"""

    # First, create the Azure OpenAI Responses client
    aifoundry_client = AzureAIAgentClient(
        async_credential=AzureCliCredential(),
        project_endpoint=os.getenv("AIF_BAS_PROJECT_ENDPOINT"),
        model_deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),        
    )

    agent = aifoundry_client.create_agent(
        agent_name="HelperAgentPython",
        instructions="You are a helpful assistant that can write and execute Python code.",
        tools=[HostedCodeInterpreterTool()])

    return agent


async def maf_agent_invocation(agent: agent_framework.ChatAgent, question: str, streaming: bool=False) -> str:
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

    agent = maf_aifoundry_agent_creation()

    # response1 = asyncio.run(maf_agent_invocation(agent, "Write a Python function that returns the Fibonacci sequence up to n and execute it with n=10.", streaming=False))
    response2 = asyncio.run(maf_agent_invocation(agent, "Tell me a story about a haunted house, then Write a Python function that returns the Fibonacci sequence up to n and execute it with n=10.", streaming=True))
    
    # print("\n\n" + "*"*80 + " RESPONSE #1")
    # print(response1)

    print("\n\n" + "*"*80 + " RESPONSE #2")
    print(response2) 

if __name__ == "__main__":
    main()
    print("Execution completed")