// See https://aka.ms/new-console-template for more information
using System.ClientModel;
using System.Formats.Asn1;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;


var cc = new OpenAI.Chat.ChatClient("gpt-4o-mini",
 new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN")!),
 new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") });


// Send a chat request using the vendor-specific client
ChatCompletion response_openAI = (await cc.CompleteChatAsync("Hi there!")).Value;

// Read the model's reply
Console.WriteLine(response_openAI.Content[0].Text);

IChatClient cc_adapter = cc.AsIChatClient();

AIAgent writer = new ChatClientAgent(
    chatClient: cc_adapter,
    name: "Writer",
    instructions: "You are a helpful writing assistant."
);

var question = "Write a short story about a haunted house.";

AgentRunResponse response = await writer.RunAsync(question);
Console.WriteLine(response.Text);

// This time we print the story with streaming
await foreach(var chunk in writer.RunStreamingAsync(question))
{
    Console.Write(chunk.Text);
}


Console.WriteLine("Hello, World!");