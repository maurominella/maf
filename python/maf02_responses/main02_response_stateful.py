import os
from dotenv import load_dotenv  # requires python-dotenv
from openai import AzureOpenAI        

def main():
    # Environment variables loading
    if not load_dotenv("./../../../config/credentials_my.env"):
        print("Environment variables not loaded, execution stopped")
        return
    else:
        print("Environment variables have been loaded ;-)")

    client = AzureOpenAI(\
        azure_endpoint = os.getenv("AZURE_OPENAI_ENDPOINT"), # Azure OpenAI resource
        api_key        = os.getenv("AZURE_OPENAI_API_KEY"),  
        api_version    = os.getenv("AZURE_OPENAI_API_VERSION") ,# at least 2024-02-15-preview
    )

    # First turn, normal question
    resp1 = client.responses.create(
        model=os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        input=[{"role": "user", "content": "Hello, how are you?"}]
    )

    # Second turn: reuse previous response ID to maintain context
    resp2 = client.responses.create(
        model=os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        previous_response_id=resp1.id,  # Passa l'ID della risposta precedente
        input=[{"role": "user", "content": "What was my previous question?"}]
    )
    print(resp2.output[0].content[0].text)
    
    
if __name__ == "__main__":
    main()