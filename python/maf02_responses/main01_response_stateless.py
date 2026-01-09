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

    client = AzureOpenAI(
        azure_endpoint = os.getenv("AZURE_OPENAI_ENDPOINT"), # Azure OpenAI resource
        api_key        = os.getenv("AZURE_OPENAI_API_KEY"),  
        api_version    = os.getenv("AZURE_OPENAI_API_VERSION") ,# at least 2024-02-15-preview,
    )

    response = client.responses.create(
        model=os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        input="Hello!", # or [{"role": "user", "content": "Hello!"}],
    )

    print(response.output_text) # or print(response.output[0].content[0].text)
    
    
if __name__ == "__main__":
    main()