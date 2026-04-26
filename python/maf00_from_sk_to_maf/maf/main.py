import os
from agent_framework import ChatAgent
from typing import Dict, Any, List

# 👇 most MAF builds expose FunctionTool here
from agent_framework import FunctionTool


agent_name   = "my_response_agent"
instructions = "you are a clever agent"
plugin_name  = "Lights"

# Hosted Tool custom class
class LightsToolDispatcher:
    """
    Same logic as your SK LightsPlugin (no sandbox; deterministic).
    Exposes two operations that match your @kernel_function names:
      - get_lights()
      - change_state(id: int, is_on: bool)
    """
    def __init__(self):
        self._lights: List[Dict[str, Any]] = [
            {"id": 0, "name": "Table Lamp",   "is_on": False},
            {"id": 1, "name": "Porch light",  "is_on": False},
            {"id": 2, "name": "Chandelier",   "is_on": False},
        ]

    async def get_lights(self, args: Dict[str, Any]) -> Dict[str, Any]:
        # Equivalent to SK get_state() → name "get_lights" in @kernel_function
        return {"lights": self._lights}

    async def change_state(self, args: Dict[str, Any]) -> Dict[str, Any]:
        lid = args.get("id")
        is_on = args.get("is_on")
        if lid is None or is_on is None:
            return {"updated": None, "error": "Missing 'id' or 'is_on'."}
        for light in self._lights:
            if light["id"] == lid:
                light["is_on"] = bool(is_on)
                return {"updated": light}
        return {"updated": None, "error": "Light not found"}

def create_maf_agent_with_lights_tool():
    """Create an agent using Azure OpenAI Responses"""
    from azure.identity.aio import AzureCliCredential
    from agent_framework.azure import AzureOpenAIResponsesClient

    # First, create the Azure OpenAI Responses client
    openairesponses_client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        api_version = os.getenv("AZURE_OPENAI_RESPONSES_API_VERSION"), # v1
    )


    dispatcher = LightsToolDispatcher()

    # 🔧 Tool 1: get_lights (no parameters)
    get_lights_tool = FunctionTool(
        name="get_lights",
        description="Gets a list of lights and their current state",
        parameters={  # JSON schema for tool input
            "type": "object",
            "properties": {},
        },
        handler=dispatcher.get_lights,   # coroutine called by the runtime
    )

    # 🔧 Tool 2: change_state (id + is_on)
    change_state_tool = FunctionTool(
        name="change_state",
        description="Changes the state of the light",
        parameters={
            "type": "object",
            "properties": {
                "id":   {"type": "integer"},
                "is_on":{"type": "boolean"},
            },
            "required": ["id", "is_on"],
        },
        handler=dispatcher.change_state,  # coroutine called by the runtime
    )

    # Then, create the agent with the hosted tool

    openairesponses_agent = openairesponses_client.create_agent(
        name="LightingAssistant",
        instructions=(
            "You can call functions 'get_lights' and 'change_state'. "
            "Use change_state with 'id' and 'is_on' to update a light."
        ),
        tools=[get_lights_tool, change_state_tool],
    )


    return openairesponses_agent

async def maf_invoke_async(agent: ChatAgent, user_message: str) -> str:
    # Depending on SDK version, the object returned exposes `.content`
    resp = await agent.invoke_async(user_message)
    return resp.content

def main():
    from dotenv import load_dotenv
    import asyncio
    
    # Environment variables loading
    if not load_dotenv("./../../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    # else:
    #     print("Environment variables have been loaded ;-)")

    agent = create_maf_agent_with_lights_tool()

    user_inputs = ["Hello", "Please toggle the porch light", "What's the status of all lights?", "Thank you"]

    for question in user_inputs:
        print(f"******\nUser: {question}")
        answer = asyncio.run(maf_invoke_async(agent, question))
        print(f"Agent: {answer}")


if __name__ == "__main__":
    main()