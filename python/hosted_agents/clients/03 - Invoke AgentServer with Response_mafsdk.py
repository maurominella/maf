# OpenAIChatClient uses Response instead of ChatCompletion, so it can be used 
# with any LLM that implements the same interface, including local ones.
from agent_framework.openai import OpenAIChatClient
import asyncio

base_url = "http://localhost:8088"
api_key = "unexisting_openai_key"
model = "unexisting_model_id"

openai_chat_client = OpenAIChatClient(
    base_url = base_url,
    api_key = api_key,
    model = model
)

from agent_framework import Agent
agent = Agent(
    client=openai_chat_client,
    # instructions="You are a helpful AI Agent that can help plan vacations for customers at random destinations.",
)

async def main():
    response = await agent.run("Plan me a day trip")
    print(f"Response: {response.messages[-1].contents[0].text}")

asyncio.run(main())