// See https://aka.ms/new-console-template for more information


using System.ComponentModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

var question = "Write a short story about a haunted house, using no more than 100 words.";
var instructions = "Write stories that are engaging and creative.";
var instructionsToolsAware = "Write stories that are engaging and creative. For each story, please include all its possible details taken from the provided tools.";

var openaiClient = new AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
    new Azure.Identity.AzureCliCredential());

var chatCompletionClient = openaiClient.GetChatClient(Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") !);

AIAgent writer = chatCompletionClient.CreateAIAgent(
    instructions: instructions,
    name: "writer");

Console.WriteLine(await writer.RunAsync(question));

#region Tools as inline functions
[Description("Provides the author of the story.")]
string GetStoryAuthor() => "Mauro Minella";

[Description("Provides the the story with title and author.")]
string FormatStory(string title, string author, string story) =>
    $"Story title: {title}\nAuthor: {author}\n\n{story}";
#endregion


// convert simple functions to AIFunctions
AIFunction aiFunctionGetStoryAuthor = AIFunctionFactory.Create(GetStoryAuthor);
var aiFunctionFormatStory = AIFunctionFactory.Create(FormatStory);

AIAgent writerWithTools = chatCompletionClient.CreateAIAgent(
    instructions: instructionsToolsAware,
    name: "writer_with_tools",
    tools: [
            aiFunctionGetStoryAuthor,
            aiFunctionFormatStory
        ]);

// Invoke the agent WITH TOOLS (and streaming support).
Console.WriteLine("\n\n+++ Running Writer with Tools +++\n");
await foreach (var update in writerWithTools.RunStreamingAsync(question))
{
    Console.Write(update);
}

Console.WriteLine("\nProgram has completed.");