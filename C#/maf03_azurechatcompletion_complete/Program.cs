// SUMMARY:
// - Agent Framework: whatever is under Microsoft.Agents.AI (Agent, Options, RunAsync, AgentRunResponse)
// - Abstraction layer: Microsoft.Extensions.AI (interfaces, e.g. IChatClient)
// - Adapter OpenAI -> IChatClient: provided by the Microsoft.Extensions.AI.OpenAI package
// - SDK vendor: OpenAI package

// SCHEMA:
// OpenAI SDK → creates the concrete client (ChatClient).
// Microsoft.Extensions.AI.OpenAI → provides the adapter (AsIChatClient()).
// Microsoft.Extensions.AI → defines the interface (IChatClient).
// Microsoft.Agents.AI (Agent Framework) → uses the interface to create and manage agents.


// - VENDOR SDK (NOT Agent Framework PACKAGE), coming from NuGet package OpenAI.
// - Here we create the vendor Native Client (OpenAI.Chat.ChatClient) to OpenAI
//   needed to talk to an OpenAI-compatible endpoint (GitHub AI in this case).
// - This is NOT Agent Framework code, but vendor-specific code, a bridge to the vendor SDK and LLM.

// setx AZURE_OPENAI_ENDPOINT "https://mmoaiswc-01.openai.azure.com/" # afer this, please restart the terminal
// setx AIF_STD_PROJECT_ENDPOINT "https://aif2stdsvhdu2.services.ai.azure.com/api/projects/aif2stdwusprj01hdu2" # afer this, please restart the terminal
// $env:AZURE_OPENAI_ENDPOINT = "https://mmoaiswc-01.openai.azure.com/" # to make it immediate in the current terminal session
// check with echo $env:AZURE_OPENAI_ENDPOINT (after restarting the terminal) that the variables are set correctly.


using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

// using Microsoft.Agents.AI.Workflows;
using OpenAI;
// using Microsoft.Extensions.AI;

var question = "Write a short story about a haunted house, using no more than 100 words.";
var instructions = "Write stories that are engaging and creative.";
var instructionsToolsAware = "Write stories that are engaging and creative. For each story, please include all its possible details taken from the provided tools.";


var client = new Azure.AI.OpenAI.AzureOpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
    new Azure.Identity.AzureCliCredential());

var chatCompletionClient = client.GetChatClient("gpt-4.1");

AIAgent writer = chatCompletionClient.CreateAIAgent(
    instructions: instructions,
    name: "writer");

// Invoke the agent and output the text result.
Console.WriteLine("\n\n+++ Running Writer with a single shot +++\n");
Console.WriteLine(await writer.RunAsync(question));

// Invoke the agent with streaming support.
Console.WriteLine("\n\n+++ Running Writer with streaming +++\n");
await foreach (var update in writer.RunStreamingAsync(question))
{
    Console.Write(update);
}

#region Tools as inline functions
[Description("Provides the author of the story.")]
string GetStoryAuthor() => "Mauro Minella";

[Description("Provides the author of the story.")]
string FormatStory(string title, string author, string story) =>
    $"Story title: {title}\nAuthor: {author}\n\n{story}";

// convert simple functions to AIFunctions
Microsoft.Extensions.AI.AIFunction aiFunctionGetStoryAuthor = Microsoft.Extensions.AI.AIFunctionFactory.Create(GetStoryAuthor);
var aiFunctionFormatStory = Microsoft.Extensions.AI.AIFunctionFactory.Create(FormatStory); // same as above
#endregion

AIAgent writerWithTools = chatCompletionClient.CreateAIAgent(
    instructions: instructionsToolsAware,
    name: "writer_with_tools",
    tools: [
            aiFunctionGetStoryAuthor,
            aiFunctionFormatStory
        ]);

// Invoke the agent WITH TOOLS (and streaming support).
Console.WriteLine("\n\n+++ Running Writer with streaming +++\n");
await foreach (var update in writerWithTools.RunStreamingAsync(question))
{
    Console.Write(update);
}