# Common Libraries
import os, sys
from dotenv import load_dotenv # requires python-dotenv
from IPython.display import Markdown, display

config_path = "../../../config" # explicit path to the config folder
sys.path.append(config_path)
from auth import acquire_bearer_token, StaticBearerTokenCredential
if not load_dotenv(f"{config_path}/credentials_my.env"):
    print("Environment variables not loaded, cell execution stopped")
else:
    print("Environment variables have been loaded ;-)")

# Global libraries - recall to declare them as "global" in the functions where they are assigned
project_endpoint = "" # must be Foundry V1 project!
deployment_name = ""
bearer_token_cognitiveservices = ""
user_cognitiveservices = ""


def main():
    print("Hello from hosted-agents!")


if __name__ == "__main__":
    main()
