# Using the pure OpenAI SDK to call the /responses endpoint exposed by the Agent Server.
from openai import AsyncOpenAI
import asyncio   

base_url = "http://localhost:8088"
api_key = "unexisting_openai_key"
model = "undexisting_model_id"

client = AsyncOpenAI(base_url=base_url, api_key=api_key)

response = asyncio.run(client.responses.create(
        model=model,
        input="Plan me a day trip",
    ))

print(f"Response: {response.output_text}")