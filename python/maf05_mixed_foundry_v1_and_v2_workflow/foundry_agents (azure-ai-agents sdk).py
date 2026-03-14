from concurrent.futures import thread

from azure.ai.projects import AIProjectClient # uv add --active azure-ai-projects --prerelease=allow 
from azure.identity import AzureCliCredential

import sys, os, asyncio
from dotenv import load_dotenv # package python-dotenv
config_path = "../../../config"
sys.path.append(config_path)
from auth import acquire_bearer_token, StaticBearerTokenCredential


# --- Global Variables ---
bearer_token = ""
openai_endpoint = ""
deployment_name = ""
openai_api_version = ""

PROJECT_ENDPOINT = ""
PERSISTENT_AGENT_ID = ""
MODEL_DEPLOYMENT = ""

messages_system = ""
messages_user = ""
project_client = None


def authentication_and_environmentsetup():
    global bearer_token
    bearer_token, user = acquire_bearer_token(
        scope="https://ai.azure.com/.default", # https://ai.azure.com/.default, https://cognitiveservices.azure.com/.default...
        auth_method="default") # default, cli, device

    if not load_dotenv(f"{config_path}/credentials_my.env"):
        print("Environment variables not loaded, cell execution stopped")
    else:
        print("Environment variables have been loaded ;-)")

    print("Bearer token:", bearer_token[:10], "...")
    print(f'User info: {user["name"]}, upn: {user["upn"]}')
    # user['raw_claims']


def init():
    global openai_endpoint, openai_api_version, messages_system, messages_user, project_client, PERSISTENT_AGENT_ID, MODEL_DEPLOYMENT_NAME, PROJECT_ENDPOINT
    openai_endpoint = os.getenv("AZURE_OPENAI_ENDPOINT") # AIF_STD_PROJECT_OPENAI_ENDPOINT DOES NOT WORK!!
    openai_api_version = os.getenv("AZURE_OPENAI_API_VERSION")
    PROJECT_ENDPOINT = os.getenv("AIF_STD_PROJECT_ENDPOINT")
    PERSISTENT_AGENT_ID = os.getenv("PERSISTENT_AGENT_ID")
    MODEL_DEPLOYMENT_NAME = os.getenv("MODEL_DEPLOYMENT_NAME")
    
    messages_system = "You are an AI assistant that helps people find information"
    messages_user = "Tell me five good names for my new pizzeria"
    project_client = AIProjectClient(
        endpoint=PROJECT_ENDPOINT,
        credential=StaticBearerTokenCredential(bearer_token)
    )

# --- V1: Persistent Agent ---
def persistent_agent_example(persistent_agent_id=PERSISTENT_AGENT_ID, input_text="What's the weather like in New York today?"):
    # 1. Crea un thread
    thread = project_client.threads.create()

    # 2. Aggiungi un messaggio utente
    project_client.threads.messages.create(
        thread_id=thread.id,
        role="user",
        content="Dimmi una barzelletta sui pirati."
    )

    # 3. Avvia un run dell’agente
    run = project_client.threads.runs.create(
        thread_id=thread.id,
        agent_id=persistent_agent_id
    )

    # 4. Attendi che il run finisca
    run = project_client.threads.runs.wait(
        thread_id=thread.id,
        run_id=run.id
    )

    # 5. Leggi la risposta
    messages = project_client.threads.messages.list(thread_id=thread.id)

    for m in messages.data:
        if m.role == "assistant":
            print(m.content[0].text)

# --- V2: Prompt Agent ---
def prompt_agent_example(agent_name="python-agent-demo", input_text="What's the weather like in New York today?"):
    v2_agent = project_client.agents.create(
    name=agent_name,
    model=MODEL_DEPLOYMENT_NAME,
    instructions="You are a helpful assistant."
    )
    v2_response = v2_agent.invoke(input_text)
    print("V2:", v2_response.output_text)


def main():
    authentication_and_environmentsetup()
    init()

    persistent_agent_example(
        persistent_agent_id=PERSISTENT_AGENT_ID, 
        input_text = "What's the weather like in Milan today?")
    
    prompt_agent_example(agent_name= "python-agent-demo01", input_text="What's the weather like in Milan today?")
    

if __name__ == "__main__":
    main()