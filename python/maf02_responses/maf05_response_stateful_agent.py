import os
import asyncio
import agent_framework # tested agent-framework=1.0.0b251216
from dotenv import load_dotenv  # tested python-dotenv=1.2.1
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.identity import AzureCliCredential, DefaultAzureCredential, get_bearer_token_provider # tested azure.identity=1.26.0b1
# from openai import OpenAI # test openai=2.13.0. However this is included in azure-ai-projects, used in other samples of this folder.

def maf_responsesai_agent_creation() -> agent_framework.ChatAgent:
    """Create an agent using Azure OpenAI Responses"""

    # First, create the Azure OpenAI Responses client
    openai_response_client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        api_version = os.getenv("AZURE_OPENAI_FOR_RESPONSES_API_VERSION"), # v1
    )

    # Then, create the agent
    openai_response_agent = openai_response_client.create_agent(
        name="HelpfulAssistant",
        instructions="You are a helpful assistant that can write and execute Python code.", 
        tools=[agent_framework.HostedCodeInterpreterTool()],
    )

    return openai_response_agent


async def maf_agent_invocation(
    agent: agent_framework.ChatAgent, 
    question: str,
    thread: agent_framework.AgentThread | None = None,
    streaming: bool=False) -> tuple[str, agent_framework.AgentThread]:

    # If no thread is provided, create a new one
    if thread is None:
        thread = agent.get_new_thread()

    response = ""
    response_id = ""
    result_thread = None

    print("Agent: ", end="", flush=True)
    if streaming:
        async for chunk in agent.run_stream(question, thread=thread):
            if chunk.text:
                print(chunk.text, end="", flush=True)
                response += chunk.text
                if chunk.response_id:
                    response_id = chunk.response_id
            result_thread = thread
    else:
        result = await agent.run(question, thread=thread)
        result_thread = thread
        print(result.text)        
        response_id = result.response_id
        response = result.text

    print("\n\n")
    
    return response, result_thread
        

def main():
    # Environment variables loading
    if not load_dotenv("./../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    else:
        print("Environment variables have been loaded ;-)")

    agent = maf_responsesai_agent_creation()

    resp1, thread = asyncio.run(maf_agent_invocation(agent, "Write a Python function that returns the Fibonacci sequence up to n and execute it with n=10.", streaming=True))
    resp2, thread = asyncio.run(maf_agent_invocation(agent, "What was my previous question?", thread=thread, streaming=True))
    
    print("\n\n" + "*"*80 + " RESPONSE #1")
    print(resp1)

    print("\n\n" + "*"*80 + " RESPONSE #2")
    print(resp2) 
    
    
if __name__ == "__main__":
    main()