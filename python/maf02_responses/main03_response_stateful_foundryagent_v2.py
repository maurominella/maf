import os
from dotenv import load_dotenv  # python-dotenv
from azure.identity import DefaultAzureCredential # azure-identity
from azure.ai.projects import AIProjectClient # azure-ai-projects>=2.0.0b1


def main():
    # Environment variables loading
    if not load_dotenv("./../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    else:
        print("Environment variables have been loaded ;-)")

    # Create the AI Project Client
    project_client = AIProjectClient(
        endpoint=os.getenv("AIF_STD_PROJECTV2_ENDPOINT"),  # Your Foundry V2 project endpoint
        credential=DefaultAzureCredential(),
        api_version=os.getenv("AZURE_OPENAI_RESPONSES_API_VERSION")
    )
    
    # Get the OpenAI client from the project (this handles API versions correctly)
    openai_response_client = project_client.get_openai_client()    

    # First turn, normal question
    resp1 = openai_response_client.responses.create(
        input =[{"role": "user", "content": "How is the weather like in Paris?"}],
        extra_body={"agent": {"name": "my-web-agent", "type": "agent_reference"}}  # Specify the agent to use
    )
    print(f"First response: {resp1.output_text}\n")

    # Second turn: reuse previous response ID to maintain context
    resp2 = openai_response_client.responses.create(
        previous_response_id=resp1.id,  # Passa l'ID della risposta precedente
        input=[{"role": "user", "content": "What was my previous question?"}],
        extra_body={"agent": {"name": "my-web-agent", "type": "agent_reference"}}  # Specify the agent to use
    )
    print(f"Second response: {resp2.output_text}\n")    

if __name__ == "__main__":
    main()