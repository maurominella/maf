﻿// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

Console.WriteLine(Environment.GetEnvironmentVariable("GITHUB_MODELS_PAT_CLASSIC")?.Substring(0, 10) + "************");

var question = "Write a short story about a haunted house.";
var cc = new OpenAI.Chat.ChatClient("gpt-4o-mini",
    new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_MODELS_PAT_CLASSIC")!),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") });

    // Send a chat request using the vendor-specific client
OpenAI.Chat.ChatCompletion response_openAI = (await cc.CompleteChatAsync(question)).Value;

// Read the model's reply
Console.WriteLine(response_openAI.Content[0].Text);

Console.WriteLine("\n\n\n\n*************\n\n\n\n\n");

IChatClient cc_adapter = cc.AsIChatClient(); // extension method to adapt to Agent Framework IChatClient

AIAgent writer = new ChatClientAgent( // Agent Framework agent wrapping the chat client
    chatClient: cc_adapter,
    name: "Writer",
    instructions: "You are a helpful writing assistant."
);


AgentResponse response = await writer.RunAsync(question);
Console.WriteLine(response.Text);

// This time we print the story with streaming
await foreach(var chunk in writer.RunStreamingAsync(question))
{
    Console.Write(chunk.Text);
}


Console.WriteLine("\nThe program has ended!");