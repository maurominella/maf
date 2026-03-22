import os, sys
import asyncio
import logging
import pkg_resources
from dotenv import load_dotenv
from agent_framework import tool

# --------------------------------------------------------------------
# GLOBAL SETUP
# --------------------------------------------------------------------

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("agent")

config_path = "."
sys.path.append(config_path)

from auth import acquire_bearer_token, StaticBearerTokenCredential

if not load_dotenv():
    logger.info("Environment variables not loaded")
else:
    logger.info("Environment variables loaded")

project_endpoint = ""
deployment_name = ""
credential = None
agent_instructions = ""


# --------------------------------------------------------------------
# VERSION LOGGING
# --------------------------------------------------------------------

def log_versions():
    for pkg in [
        "agent-framework-core",
        "agent-framework-azure-ai",
        "azure-ai-agentserver-agentframework",
        "azure-ai-agents",
        "azure-ai-projects",
    ]:
        try:
            v = pkg_resources.get_distribution(pkg).version
            logger.info(f"{pkg} == {v}")
        except Exception:
            logger.info(f"{pkg} NOT INSTALLED")


# --------------------------------------------------------------------
# INIT
# --------------------------------------------------------------------

def init():
    from azure.identity import DefaultAzureCredential
    global project_endpoint, deployment_name, credential, agent_instructions

    project_endpoint = "https://aifv2-01-std-foundry.services.ai.azure.com/api/projects/aifv2-01-std-foundryproj01-default"
    deployment_name = "gpt-4o"
    credential = DefaultAzureCredential()

    agent_instructions = """
    You MUST ALWAYS call the get_local_date_time tool.
    You MUST NOT answer directly.
    If you cannot determine the timezone, call the tool with "UTC".
    Never return an empty answer.
    """


# --------------------------------------------------------------------
# TOOL
# --------------------------------------------------------------------

@tool
def get_local_date_time(iana_timezone: str) -> str:
    logger.info(f"get_local_date_time CALLED with tz={iana_timezone!r}")

    from datetime import datetime
    from zoneinfo import ZoneInfo

    try:
        tz = ZoneInfo(iana_timezone)
        current_time = datetime.now(tz)
        result = f"The current date and time in {iana_timezone} is {current_time.strftime('%A, %B %d, %Y at %I:%M %p %Z')}"
        logger.info(f"get_local_date_time RESULT: {result}")
        return result
    except Exception as e:
        logger.exception(f"Error in get_local_date_time: {e}")
        return f"Error: Unable to get time for timezone '{iana_timezone}'. {str(e)}"


# --------------------------------------------------------------------
# AGENT FACTORY FOR HOSTED AGENTSERVER
# --------------------------------------------------------------------

async def _create_agent_async():
    from agent_framework.azure import AzureOpenAIResponsesClient
    from agent_framework import Agent

    logging.getLogger("agent_framework").setLevel(logging.DEBUG)
    logging.getLogger("agent_framework.azure").setLevel(logging.DEBUG)
    logging.getLogger("agent_framework.openai").setLevel(logging.DEBUG)

    client = AzureOpenAIResponsesClient(
        project_endpoint=project_endpoint,
        credential=credential,
        deployment_name=deployment_name,
    )

    agent = Agent(
        client=client,
        instructions=agent_instructions,
        tools=[get_local_date_time],
    )

    return agent


def get_agent():
    """
    This is the ONLY entrypoint the Hosted AgentServer uses.
    It MUST return an Agent instance synchronously.
    """
    logger.info("get_agent() CALLED — initializing agent")
    init()
    log_versions()
    return asyncio.run(_create_agent_async())