// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

var question = "Write a short story about a haunted house.";

var cc = new OpenAI.Chat.ChatClient("gpt-4o-mini",
 new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN")!),
 new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") });

 // Send a chat request using the vendor-specific client
ChatCompletion response_openAI = (await cc.CompleteChatAsync("Hi there!")).Value;

// Read the model's reply
Console.WriteLine(response_openAI.Content[0].Text);


// Adapt to IChatClient
IChatClient cc_adapter = cc.AsIChatClient();

// Writer agent
AIAgent writer = new ChatClientAgent(
    chatClient: cc_adapter,
    name: "Writer",
    instructions: "You are a helpful writing assistant."
);
AgentRunResponse story = await writer.RunAsync(question);
Console.WriteLine(story.Text);


// Translator agent
AIAgent translator = new ChatClientAgent(
    chatClient: cc_adapter,
    name: "Translator",
    instructions: "Translate the text to Italian."
);
AgentRunResponse response = await translator.RunAsync(story.Text);

Console.WriteLine(response.Text);

Console.WriteLine("\nProgram has completed.");