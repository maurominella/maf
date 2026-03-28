from typing import Any, AsyncGenerator

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

import os
import logging

# (Opzionale) se in locale vuoi caricare anche il .env:
# from dotenv import load_dotenv
# load_dotenv()

logger = logging.getLogger("env-dump")
if not logger.handlers:
    logging.basicConfig(level=logging.INFO)

def _mask(value: str, show: int = 6) -> str:
    """Maschera un valore evitando di loggare segreti interi."""
    if value is None:
        return "<NOT SET>"
    v = str(value)
    if len(v) <= show:
        return v
    return f"{v[:show]}...({len(v)} chars)"

def log_required_env():
    aif = os.getenv("AIF_STD_PROJECT_ENDPOINT")
    model = os.getenv("MODEL_DEPLOYMENT_NAME")

    logger.info("AIF_STD_PROJECT_ENDPOINT = %s", _mask(aif))
    logger.info("MODEL_DEPLOYMENT_NAME     = %s", _mask(model))

    # Se vuoi anche fallire hard quando mancano:
    # if not aif or not model:
    #     raise RuntimeError("Missing required env vars: AIF_STD_PROJECT_ENDPOINT and/or MODEL_DEPLOYMENT_NAME")

class hostedagent03_echoagent(BaseAgent):
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
        """Initialize the hostedagent03_echoagent.

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

        if not normalized:
            response_text = "Hello! I'm a custom echo agent. Send me a message and I'll echo it back."
        else:
            last_message = normalized[-1]
            if last_message.text:
                response_text = f"{self.echo_prefix}{last_message.text}"
            else:
                response_text = f"{self.echo_prefix}[Non-text message received]"

        # --- STREAMING MODE ----------------------------------------------------
        if stream and session is not None:
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
    

def create_agent() -> hostedagent03_echoagent:
    agent = hostedagent03_echoagent(
        name="mauromi_agent_echo",
        description="A simple agent that echoes user messages",
        custom_member="🔊 Echo: ",
    )
    return agent


if __name__ == "__main__":
    log_required_env()
    from_agent_framework(create_agent()).run()