import os, asyncio
from dotenv import load_dotenv  # requires python-dotenv
from azure.identity.aio import AzureCliCredential
import agent_framework
from agent_framework import HostedCodeInterpreterTool


async def delete_all_agents_and_threads_async():
    """Delete all agents and threads from Azure AI Foundry project"""
    from azure.ai.projects.aio import AIProjectClient
    from azure.identity.aio import AzureCliCredential

    global project_endpoint, credential

    # ask confirmation
    confirmation = input(f"Are you sure you want to delete ALL agents and threads from the project at {project_endpoint}? (y/n): ")
    if confirmation[0].lower() != "y":
        print("Deletion cancelled.")
        return
    
    project_client = AIProjectClient(
        credential=credential,
        endpoint=project_endpoint
    )

    try:
        # Collect all thread IDs first
        print("Fetching all threads...")
        thread_ids = []
        async for thread in project_client.agents.threads.list():
            thread_ids.append(thread.id)

        # Delete all threads
        print(f"Fetching all the {len(thread_ids)} threads...")
        threads = project_client.agents.threads
        thread_count = 0
        for thread_id in thread_ids:
            try:
                print(f"Deleting thread # {thread_count+1}/{len(thread_ids)}: {thread_id}...")
                await project_client.agents.threads.delete(thread_id = thread_id)
                thread_count += 1
            except Exception as e:
                print(f"Error deleting thread {thread_id}: {e}")
        print(f"Total threads deleted: {thread_count}")
        
        # Delete all agents
        print("\nFetching all agents...")
        agents = project_client.agents.list_agents()
        agent_count = 0
        agents_ids = []
        async for agent in agents:
            agents_ids.append((agent.name,agent.id))

        for agent_id in agents_ids:
            try:
                print(f"Deleting agent # {agent_count+1}/{len(agents_ids)}: {agent_id[0]} ({agent_id[1]})...")
                await project_client.agents.delete_agent(agent_id[1])
                agent_count += 1
            except Exception as e:
                print(f"Error deleting agent {agent_id[1]}: {e}")
        print(f"Total agents deleted: {agent_count}")
        
    finally:
        project_client.close()
        print("\nCleanup completed!")

async def maf_aifoundry_agent_creation_simple(agent_name: str) -> agent_framework.ChatAgent:
    """Create an agent using Azure OpenAI Responses"""
    from agent_framework.azure import AzureAIAgentClient
    global project_endpoint, instructions, model_deployment_name, credential

    project_client = AzureAIAgentClient(
        async_credential=credential,
        project_endpoint=project_endpoint,
        model_deployment_name = model_deployment_name)

    agent = project_client.create_agent(
        agent_name=agent_name,
        instructions=instructions,
        tools=[HostedCodeInterpreterTool()],
        )

    # Store references for cleanup
    agent._credential = credential
    agent._project_client = project_client
    
    return agent

async def maf_aifoundry_agent_creation_full(agent_name: str) -> agent_framework.ChatAgent:
    """Create an agent using Azure OpenAI Responses"""

    from azure.ai.projects.aio import AIProjectClient
    from agent_framework.azure import AzureAIAgentClient

    global model_deployment_name, instructions, project_endpoint, credential

    project_client = AIProjectClient(
        credential=credential,
        endpoint=project_endpoint)

    created_agent = await project_client.agents.create_agent(
        name = agent_name,
        model = model_deployment_name,
        instructions= instructions,
        tools=[{"type": "code_interpreter"}],
        )
    
    agent_client = AzureAIAgentClient(
        project_client=project_client,
        agent_id=created_agent.id)
    
    agent = agent_framework.ChatAgent(
        chat_client=agent_client,
        # tools = [HostedCodeInterpreterTool()], # not needed, tool is already configured in the agent
        store = True)

    # Store references for cleanup
    agent._credential = credential
    agent._project_client = project_client
    
    return agent


async def maf_agent_invocation(agent: agent_framework.ChatAgent, question: str, streaming: bool=False) -> str:
    response = ""
    print("Agent answer: ", end="", flush=True)
    if streaming:
        stream = None
        try:
            stream = agent.run_stream(question)
            async for chunk in stream:
                if chunk.text:
                    print(chunk.text, end="", flush=True)
                    response += chunk.text
        finally:
            # Ensure the stream is properly closed
            if stream is not None and hasattr(stream, 'aclose'):
                await stream.aclose()
    else:
        result = await agent.run(question)
        print(result.text)
        response = result.text
        
    return response



async def main_async():
    """ Environment variables loading """
    global project_endpoint, model_deployment_name, instructions, credential

    if not load_dotenv("./../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    else:
        print("Environment variables have been loaded ;-)")

    question = "Tell me a story about a haunted house, then Write a Python function that returns the Fibonacci sequence up to n and execute it with n=10."
    instructions = "You are a helpful assistant that can write and execute Python code."
    agent_name = "HelperAgentPython"
    project_endpoint=os.getenv("AIF_BAS_PROJECT_ENDPOINT")
    model_deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME")
    credential = AzureCliCredential()

    agent_simple = await maf_aifoundry_agent_creation_simple(agent_name=agent_name+"_simple")
    try:
        response_simple = await maf_agent_invocation(agent_simple, question, streaming=True)
        print(f'\n\n{"*"*80} RESPONSE SIMPLE:\n{response_simple}')
        response_simple = await maf_agent_invocation(agent_simple, "what was my last question?", streaming=True)
        print(f'\n\n{"*"*80} RESPONSE SIMPLE:\n{response_simple}')
    finally:
        # Properly close all async resources
        print("Cleaning up agent_simple resources...")
        
        # Close the project client
        if hasattr(agent_simple, '_project_client'):
            if hasattr(agent_simple._project_client, 'close'):
                await agent_simple._project_client.close()
        
        # Close the chat_client
        if hasattr(agent_simple, 'chat_client'):
            if hasattr(agent_simple.chat_client, 'close'):
                await agent_simple.chat_client.close()
        
        # Close the credential
        if hasattr(agent_simple, '_credential'):
            await agent_simple._credential.close()

    agent_full = await maf_aifoundry_agent_creation_full(agent_name=agent_name+"_full")
    try:
        response_full = await maf_agent_invocation(agent_full, question, streaming=True)
        print(f'\n\n{"*"*80} RESPONSE FULL:\n{response_full}')
        response_full = await maf_agent_invocation(agent_full, "what was my last question?", streaming=True)
        print(f'\n\n{"*"*80} RESPONSE FULL:\n{response_full}')
    finally:
        # Properly close all async resources
        print("Cleaning up agent_full resources...")
        
        # Close the project client
        if hasattr(agent_full, '_project_client'):
            if hasattr(agent_full._project_client, 'close'):
                await agent_full._project_client.close()
        
        # Close the chat_client
        if hasattr(agent_full, 'chat_client'):
            if hasattr(agent_full.chat_client, 'close'):
                await agent_full.chat_client.close()
        
        # Close the credential
        if hasattr(agent_full, '_credential'):
            await agent_full._credential.close()

    await delete_all_agents_and_threads_async()


if __name__ == "__main__":    
    asyncio.run(main_async())
    print("Execution completed")