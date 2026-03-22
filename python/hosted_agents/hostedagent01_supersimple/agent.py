from typing import Any
from collections.abc import AsyncIterable

from agent_framework import (
    AgentRunResponse,
    AgentRunResponseUpdate,
    AgentThread,
    BaseAgent,
    ChatMessage,
    Role,
    TextContent,
)
from azure.ai.agentserver.agentframework import from_agent_framework


class hostedagent01_supersimple(BaseAgent):
    """A simple custom agent that echoes user messages with a prefix.

    This demonstrates how to create a fully custom agent by extending BaseAgent
    and implementing the required run() and run_stream() methods.
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
        messages: str | ChatMessage | list[str] | list[ChatMessage] | None = None,
        *,
        thread: AgentThread | None = None,
        **kwargs: Any,
    ) -> AgentRunResponse:
        """Execute the agent and return a complete response.

        Args:
            messages: The message(s) to process.
            thread: The conversation thread (optional).
            **kwargs: Additional keyword arguments.

        Returns:
            An AgentRunResponse containing the agent's reply.
        """
        normalized_messages = self._normalize_messages(messages)

        if not normalized_messages:
            response_message = ChatMessage(
                role=Role.ASSISTANT,
                contents=[TextContent(text="Hello! I'm a custom echo agent. Send me a message and I'll echo it back.")],
            )
        else:
            last_message = normalized_messages[-1]
            if last_message.text:
                echo_text = f"{self.echo_prefix}{last_message.text}"
            else:
                echo_text = f"{self.echo_prefix}[Non-text message received]"

            response_message = ChatMessage(role=Role.ASSISTANT, contents=[TextContent(text=echo_text)])

        if thread is not None:
            await self._notify_thread_of_new_messages(thread, normalized_messages, response_message)

        return AgentRunResponse(messages=[response_message])

    async def run_stream(
        self,
        messages: str | ChatMessage | list[str] | list[ChatMessage] | None = None,
        *,
        thread: AgentThread | None = None,
        **kwargs: Any,
    ) -> AsyncIterable[AgentRunResponseUpdate]:
        """Execute the agent and yield streaming response updates.

        Args:
            messages: The message(s) to process.
            thread: The conversation thread (optional).
            **kwargs: Additional keyword arguments.

        Yields:
            AgentRunResponseUpdate objects containing chunks of the response.
        """
        normalized_messages = self._normalize_messages(messages)

        if not normalized_messages:
            response_text = "Hello! I'm a custom echo agent. Send me a message and I'll echo it back."
        else:
            last_message = normalized_messages[-1]
            if last_message.text:
                response_text = f"{self.echo_prefix}{last_message.text}"
            else:
                response_text = f"{self.echo_prefix}[Non-text message received]"

        words = response_text.split()
        for i, word in enumerate(words):
            chunk_text = f" {word}" if i > 0 else word
            yield AgentRunResponseUpdate(
                contents=[TextContent(text=chunk_text)],
                role=Role.ASSISTANT,
            )

        if thread is not None:
            complete_response = ChatMessage(role=Role.ASSISTANT, contents=[TextContent(text=response_text)])
            await self._notify_thread_of_new_messages(thread, normalized_messages, complete_response)


def create_agent() -> hostedagent01_supersimple:
    agent = hostedagent01_supersimple(
        name="mauromi_agent_supersimple",
        description="A simple agent that returns a static message",
        custom_member="🔊 Result: ",
    )
    return agent


if __name__ == "__main__":
    from_agent_framework(create_agent()).run()
