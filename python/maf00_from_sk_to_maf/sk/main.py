import os
from semantic_kernel.agents import AzureResponsesAgent

agent_name   = "my_response_agent"
instructions = "you are a clever agent"
plugin_name  = "Lights"

class LightsPlugin:
    from typing import Annotated
    from semantic_kernel.functions import kernel_function
   
    def __init__(self):
        self.lights = [
        {"id": 0, "name": "Table Lamp", "is_on": False},
        {"id": 1, "name": "Porch light", "is_on": False},
        {"id": 2, "name": "Chandelier", "is_on": False},]
 
    @kernel_function(
        name="get_lights", # <<<=== DIFFERENT FROM THE FUNCTION NAME <get_state>, which will be ignored
        description="Gets a list of lights and their current state",
    )
    def get_state(
        self,
    ) -> Annotated[str, "the output is a string"]:
        """Gets a list of lights and their current state."""
        return self.lights
 
    @kernel_function(
        name="change_state",
        description="Changes the state of the light",
    )
    def change_state(
        self,
        id: int,
        is_on: bool,
    ) -> Annotated[str, "the output is a string"]:
        """Changes the state of the light."""
        for light in self.lights:
            if light["id"] == id:
                light["is_on"] = is_on
                return light
        return None

def create_responses_agent():    
    from semantic_kernel.connectors.ai.open_ai import AzureOpenAISettings

    # deployment name can be implicit if the following environment variables are set
    os.environ['AZURE_OPENAI_ENDPOINT'] = os.environ['AZURE_OPENAI_ENDPOINT'] # variable already exists, same name
    os.environ['AZURE_OPENAI_API_KEY']  = os.environ['AZURE_OPENAI_API_KEY'] # variable already exists, same name
    os.environ['AZURE_OPENAI_RESPONSES_DEPLOYMENT_NAME'] = os.environ['AZURE_OPENAI_CHAT_DEPLOYMENT_NAME'] # variable already exists, different name

    # Set up the client and model using Azure OpenAI Resources
    client = AzureResponsesAgent.create_client()

    # AzureOpenAISettings()    

    agent = AzureResponsesAgent(
        ai_model_id=AzureOpenAISettings().responses_deployment_name,
        client=client,
        instructions=instructions,
        name=agent_name,
        plugins=[LightsPlugin()]
    )
    return agent



async def sk_agent_invocation_async(agent: AzureResponsesAgent, question: str, streaming: bool=False) -> str:
    from semantic_kernel.agents import ChatHistoryAgentThread
    thread:ChatHistoryAgentThread = None
    response = ""
    if streaming:
        print("Agent: ", end="", flush=True)
        async for chunk in agent.invoke_stream(question):
            if chunk.content:
                print(chunk.content, end="", flush=True)
                response += chunk.content.__str__()
    else:
        async for r in agent.invoke(messages=question, thread=thread):
            thread = r.thread
            response = r.content
            # print(f"Agent response name from <{r.name}>: {response}")

    return response



def main():
    from dotenv import load_dotenv
    import asyncio
    
    # Environment variables loading
    if not load_dotenv("./../../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    # else:
    #     print("Environment variables have been loaded ;-)")

    agent = create_responses_agent()

    user_inputs = ["Hello", "Please toggle the porch light", "What's the status of all lights?", "Thank you"]

    for question in user_inputs:
        print(f"******\nUser: {question}")
        answer = asyncio.run(sk_agent_invocation_async(agent, question, streaming=True))
        print(f"\nAnswer: {answer}\n\n")


if __name__ == "__main__":
    main()