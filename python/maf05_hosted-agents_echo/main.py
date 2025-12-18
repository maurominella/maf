import asyncio
from dotenv import load_dotenv


from azure.ai.agentserver.agentframework import from_agent_framework

from agent_framework import (
    BaseAgent,
    ChatMessage,
    TextContent,
    Role,
    AgentThread,
    AgentRunResponse,
)

from typing import Any


class EchoAgent(BaseAgent):
    """A simple custom agent that echoes user messages with a prefix.

    This demonstrates how to create a fully custom agent by extending BaseAgent
    and implementing the required run() and run_stream() methods.
    """

    def __init__(
        self,
        *,
        name: str | None = None,
        description: str | None = None,
        echo_prefix: str = "Echo: ",
        **kwargs: Any,
    ) -> None:
        """Initialize the EchoAgent.

        Args:
            name: The name of the agent.
            description: The description of the agent.
            echo_prefix: The prefix to add to echoed messages.
            **kwargs: Additional keyword arguments passed to BaseAgent.
        """
        self.echo_prefix = echo_prefix

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
        # Normalize input messages to a list
        normalized_messages = self._normalize_messages(messages)

        if not normalized_messages:
            response_message = ChatMessage(
                role=Role.ASSISTANT,
                contents=[TextContent(text="Hello! I'm a custom echo agent. Send me a message and I'll echo it back.")],
            )
        else:
            # For simplicity, echo the last user message
            last_message = normalized_messages[-1]
            if last_message.text:
                echo_text = f"{self.echo_prefix}{last_message.text}"
            else:
                echo_text = f"{self.echo_prefix}[Non-text message received]"

            response_message = ChatMessage(role=Role.ASSISTANT, contents=[TextContent(text=echo_text)])

        # Notify the thread of new messages if provided
        if thread is not None:
            await self._notify_thread_of_new_messages(thread, normalized_messages, response_message)

        return AgentRunResponse(messages=[response_message])
    
def create_agent() -> EchoAgent:
    agent = EchoAgent(
        name="EchoBot", description="A simple agent that echoes messages with a prefix", echo_prefix="🔊 Echo: "
    )
    return agent

def main():
    print("Hello from maf05-hosted-agents-echo!")
    from_agent_framework(lambda _: create_agent()).run()


if __name__ == "__main__":
    load_dotenv()
    main()