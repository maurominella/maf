from typing import Any

from agent_framework import (
    AgentResponse,
    AgentResponseUpdate,
    AgentSession,
    BaseAgent,
    Content,
    Message,
    Role,
    normalize_messages,
)
from azure.ai.agentserver.agentframework import from_agent_framework


class hostedagent01_supersimple(BaseAgent):
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
        """Initialize the hostedagent01_supersimple.

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

    async def run(
        self,
        messages: str | Message | list[str] | list[Message] | None = None,
        *,
        stream: bool = False,
        session: AgentSession | None = None,
        **kwargs: Any,
    ):
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

        if stream:
            async def _stream():
                words = response_text.split()
                for i, word in enumerate(words):
                    chunk_text = f" {word}" if i > 0 else word
                    yield AgentResponseUpdate(
                        contents=[Content(type="text", text=chunk_text)],
                        role="assistant",
                    )
            return _stream()

        response_message = Message(role="assistant", text=response_text)
        return AgentResponse(messages=[response_message])


def create_agent() -> hostedagent01_supersimple:
    agent = hostedagent01_supersimple(
        name="mauromi_agent_supersimple",
        description="A simple agent that returns a static message",
        custom_member="🔊 Result: ",
    )
    return agent


if __name__ == "__main__":
    from_agent_framework(create_agent()).run()