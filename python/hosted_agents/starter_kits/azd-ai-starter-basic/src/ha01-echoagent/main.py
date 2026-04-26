import logging
import os
from typing import Any, AsyncGenerator

from azure.monitor.opentelemetry import configure_azure_monitor
from agent_framework import (
    AgentResponse,
    AgentResponseUpdate,
    AgentSession,
    BaseAgent,
    Content,
    Message,
    ResponseStream,
    Role,
    normalize_messages,
)
from azure.ai.agentserver.agentframework import from_agent_framework

# Variables injection
"""
Run the following commands from the azd project folder:
```bash
azd env set AIF_STD_PROJECT_ENDPOINT "https://foundry7159.services.ai.azure.com/api/projects/aif7159-standard-agent-project"
azd env set MODEL_DEPLOYMENT_NAME "gpt-4o"
azd env set APPLICATIONINSIGHTS_CONNECTION_STRING "InstrumentationKey=***;IngestionEndpoint=ht.."
```
"""


# Configure logging - INFO for this module only, WARNING for everything else
logging.basicConfig(level=logging.WARNING) # this is the "father" logger, set to WARNING to avoid too much noise from other modules
logger = logging.getLogger(__name__) # this is the "child" logger for our module (this module)
logger.setLevel(logging.INFO) # we set the child logger to INFO to get more detailed logs from our module
if not logger.handlers: # avoid adding multiple handlers if this code is reloaded multiple times (e.g. during development)
    _handler = logging.StreamHandler()
    _handler.setLevel(logging.INFO)
    logger.addHandler(_handler)
    # propagate=True (default) so logs also reach the root logger,
    # where configure_azure_monitor() attaches the App Insights handler

# Configure Azure Monitor if connection string is available (injected by Foundry when App Insights is connected)
if os.environ.get("APPLICATIONINSIGHTS_CONNECTION_STRING"):
    configure_azure_monitor()

class hostedagent_echoagent(BaseAgent):
    """A simple custom agent that echoes user messages with a prefix.

    This demonstrates how to create a fully custom agent by extending BaseAgent
    and implementing the required run() method.
    """

    def __init__(
        self,
        *,
        name: str | None = None,
        description: str | None = None,
        custom_member: str = "Echo: ",
        **kwargs: Any,
    ) -> None:
        """Initialize the hostedagent_echoagent.

        Args:
            name: The name of the agent.
            description: The description of the agent.
            custom_member: A custom member variable for the agent.
            **kwargs: Additional keyword arguments passed to BaseAgent.
        """
        self.echo_prefix = custom_member
        super().__init__(
            name=name,
            description=description,
            **kwargs,
        )

    def run(
        self,
        messages: str | Message | list[str] | list[Message] | None = None,
        *,
        stream: bool = False,
        session: AgentSession | None = None,
        **kwargs: Any,
    ) -> AgentResponse | AsyncGenerator[AgentResponseUpdate, None]:
        """Execute the agent and return a complete response or a streaming generator.

        Args:
            messages: The message(s) to process.
            stream: If True, return an async generator of AgentResponseUpdate. If False, return AgentResponse.
            session: The conversation session (optional).
            **kwargs: Additional keyword arguments.

        Returns:
            When stream=False: An AgentResponse containing the agent's reply.
            When stream=True: An async generator yielding AgentResponseUpdate chunks.
        """
        normalized = normalize_messages(messages)

        # this is written to stderr, which is visible in the agent logs in Foundry - useful for debugging
        logger.info("run() called: stream=%s, messages=%d", stream, len(normalized) if normalized else 0)

        if not normalized:
            response_text = "Hello! I'm a custom echo agent. Send me a message and I'll echo it back."
        else:
            last_message = normalized[-1]
            if last_message.text:
                response_text = f"{self.echo_prefix}{last_message.text}"
            else:
                response_text = f"{self.echo_prefix}[Non-text message received]"

        # --- STREAMING MODE ----------------------------------------------------
        if stream:
            async def generator():
                words = response_text.split()
                for i, word in enumerate(words):
                    chunk_text = f" {word}" if i > 0 else word
                    yield AgentResponseUpdate(
                        contents=[Content(type="text", text=chunk_text)],
                        role="assistant",
                    )

            # Return a valid async generator for streaming mode
            return ResponseStream(generator(), finalizer=AgentResponse.from_updates) # return generator()

        # --- NON-STREAMING MODE ------------------------------------------------
        async def _respond():
            return AgentResponse(messages=[Message(role="assistant", text=response_text)])
        return _respond()
    

def create_agent() -> hostedagent_echoagent:
    agent = hostedagent_echoagent(
        name="mauromi_agent_echo",
        description="A simple agent that echoes user messages",
        custom_member="🔊 Echo: ",
    )
    return agent

if __name__ == "__main__":
    echo_agent = create_agent()
    from_agent_framework(echo_agent).run()