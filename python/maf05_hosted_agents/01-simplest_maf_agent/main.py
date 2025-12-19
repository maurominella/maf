import os, asyncio
import agent_framework # tested agent-framework=1.0.0b251216
from agent_framework.azure import AzureOpenAIResponsesClient # same package as the previous line
from azure.identity import AzureCliCredential # tested azure.identity=1.26.0b1
from dotenv import load_dotenv  # tested python-dotenv=1.2.1
from azure.ai.agentserver.agentframework import from_agent_framework  # pyright: ignore[reportUnknownVariableType]

if not load_dotenv("./../../../../config/credentials_my.env"):
    print("Environment variables not loaded, execution stopped")
    exit()
else:
    print("Environment variables have been loaded ;-)")

os.environ["AZURE_AI_PROJECT_ENDPOINT"] = os.getenv("AIF_STD_PROJECTV2_ENDPOINT")
os.environ["AZURE_SUBSCRIPTION_ID"] = os.getenv("AZURE_SUBSCRIPTION_ID")
os.environ["AZURE_RESOURCE_GROUP_NAME"] = "aifv2-08-std-rg"
os.environ["AZURE_AI_PROJECT_NAME"] = "aifv2-08-std-foundryproj01-default"


def create_agent():    
    openai_response_client = AzureOpenAIResponsesClient(
        credential=AzureCliCredential(),
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        deployment_name = os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"),
        api_version = os.getenv("AZURE_OPENAI_RESPONSES_API_VERSION"), # v1
    )

    maf_openai_response_agent = openai_response_client.create_agent(
        name="HelpfulAssistant",
        instructions="You are a helpful assistant",
    )

    return maf_openai_response_agent


def main():
    agent = create_agent()

    # Run the agent normally
    result = asyncio.run(agent.run("Hi there! Can you help me?"))
    print(result.text)

    # Required environment variables for hosted agent
    required_vars = {
        "AZURE_AI_PROJECT_ENDPOINT": "Your project endpoint (e.g., https://your-project.api.azureml.ms)",
        "AZURE_SUBSCRIPTION_ID": "Your Azure subscription ID",
        "AZURE_RESOURCE_GROUP_NAME": "Your resource group name",
        "AZURE_AI_PROJECT_NAME": "Your project name",
        # "APPLICATIONINSIGHTS_CONNECTION_STRING": "Your Application Insights connection string", # optional
        # "AGENT_PROJECT_RESOURCE_ID": "Your agent project resource ID", # optional
    }

    missing_vars = [var for var in required_vars if not os.getenv(var)]
    if missing_vars:
        print("\n⚠️  Missing environment variables for hosted agent:")
        for var in missing_vars:
            print(f"  - {var}: {required_vars[var]}")
        print("\nPlease add these to your .env file or set them as environment variables.")
        
    # Run the agent as a hosted agent
    from_agent_framework(lambda _: create_agent()).run()


if __name__ == "__main__":
    main()