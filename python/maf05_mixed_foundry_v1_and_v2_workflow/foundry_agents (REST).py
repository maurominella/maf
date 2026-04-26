import asyncio
import httpx
from azure.identity import AzureCliCredential


# ============================================================
# CONFIGURAZIONE
# ============================================================

PROJECT_ENDPOINT = "https://<your-project>.projects.azure.com"
PERSISTENT_AGENT_ID = "<your-persistent-agent-id>"
MODEL_DEPLOYMENT = "<your-model-deployment>"


# ============================================================
# TOKEN
# ============================================================

async def get_token():
    """
    Ottiene un token Azure AD valido per chiamare Azure AI Foundry.
    """
    cred = AzureCliCredential()
    token = cred.get_token("https://cognitiveservices.azure.com/.default").token
    return token


# ============================================================
# V1 — INVOCATION DI UN AGENTE PERSISTENTE (CLASSIC)
# ============================================================

async def run_v1_persistent_agent(prompt: str):
    """
    Invoca un agente Foundry V1 (Persistent / Classic) tramite REST.
    """
    token = await get_token()

    url = f"{PROJECT_ENDPOINT}/agents/{PERSISTENT_AGENT_ID}/invocations"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    payload = {"input": prompt}

    async with httpx.AsyncClient() as client:
        r = await client.post(url, json=payload, headers=headers)
        r.raise_for_status()
        return r.json()


# ============================================================
# V2 — CREAZIONE DI UN PROMPT AGENT
# ============================================================

async def create_v2_prompt_agent(name: str, instructions: str):
    """
    Crea un nuovo Prompt Agent (V2) tramite REST.
    """
    token = await get_token()

    url = f"{PROJECT_ENDPOINT}/agents"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    payload = {
        "name": name,
        "model": MODEL_DEPLOYMENT,
        "instructions": instructions
    }

    async with httpx.AsyncClient() as client:
        r = await client.post(url, json=payload, headers=headers)
        r.raise_for_status()
        agent = r.json()
        return agent["id"]


# ============================================================
# V2 — INVOCATION DI UN PROMPT AGENT
# ============================================================

async def run_v2_prompt_agent(agent_id: str, prompt: str):
    """
    Invoca un Prompt Agent (V2) tramite REST.
    """
    token = await get_token()

    url = f"{PROJECT_ENDPOINT}/agents/{agent_id}/invocations"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    payload = {"input": prompt}

    async with httpx.AsyncClient() as client:
        r = await client.post(url, json=payload, headers=headers)
        r.raise_for_status()
        return r.json()


# ============================================================
# ESEMPIO DI UTILIZZO
# ============================================================

async def main():
    print("\n=== V1 Persistent Agent ===")
    v1_response = await run_v1_persistent_agent("Tell me a joke about a pirate.")
    print(v1_response)

    print("\n=== V2 Create Prompt Agent ===")
    agent_id = await create_v2_prompt_agent(
        name="python-agent-demo",
        instructions="You are a helpful assistant."
    )
    print("Created agent:", agent_id)

    print("\n=== V2 Prompt Agent Invocation ===")
    v2_response = await run_v2_prompt_agent(agent_id, "What is the weather in Milan today?")
    print(v2_response)


if __name__ == "__main__":
    asyncio.run(main())