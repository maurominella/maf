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

// setx GITHUB_TOKEN ""github_pat_11AHXNH..." # afer this, please restart the terminal
// setx AIF_STD_PROJECT_ENDPOINT "https://aif2stdsvhdu2.services.ai.azure.com/api/projects/aif2stdwusprj01hdu2" # afer this, please restart the terminal
// $env:AZURE_OPENAI_CHAT_DEPLOYMENT_NAME = "gpt-4o" # to make it immediate in the current terminal session
// check with echo $env:AIF_BASPROJECT_ENDPOINT (after restarting the terminal) that the variables are set correctly.


using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

var question = "Write a short story about a haunted house, using no more than 100 words.";
var instructions = "Write stories that are engaging and creative.";
var instructionsToolsAware = "Write stories that are engaging and creative. For each story, please include all its possible details taken from the provided tools.";

var e = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!;

// Create the vendor-specific ChatClient for GitHub AI
var cc = new OpenAI.Chat.ChatClient(
            "gpt-4o-mini",
            new System.ClientModel.ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN")!),
            new OpenAI.OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") });

// Send a chat request using the vendor-specific client
OpenAI.Chat.ChatCompletion response_openAI = (await cc.CompleteChatAsync("Hi there!")).Value;

// Read the model's reply
Console.WriteLine(response_openAI.Content[0].Text);


// - The ADAPTER PROVIDER (from package Microsoft.Extensions.AI.OpenAI) exposes the extension AsIChatClient()
//   that builds a wrapper around the vendor-specific ChatClient (cc) to expose the standard IChatClient interface.
// - cc_adapter implements IChatClient interface through the adapter --> a concrete object that delegates all calls to cc.
// - The namespace does not always match the NuGet package name:
//   - AsIChatClient(...) is provided by the NuGet package Microsoft.Extensions.AI.OpenAI, even though 
//   - the class/method may live in the namespace Microsoft.Extensions.AI or in a static class called OpenAIClientExtensions.
// - This is needed to use MAF that does not know OpenAI, it only knows the standard IChatClient interface 
//   defined in Microsoft.Extensions.AI namespace of the Microsoft.Extensions.AI.OpenAI package.
// - Now Using AsIChatClient method from Microsoft.Extensions.AI.OpenAI package...ì
//   ...to adapt the OpenAI ChatClient to the MAF IChatClient interface defined in Microsoft.Extensions.AI

Microsoft.Extensions.AI.IChatClient cc_adapter = Microsoft.Extensions.AI.OpenAIClientExtensions.AsIChatClient(cc);


// +++  FROM HERE ON, WE USE MAF PACKAGE ONLY  +++

Console.WriteLine("\n\n+++ Running writer +++\n");

// Create a ChatClientAgent object, which is a general-purpose agent that can talk to any IChatClient implementation.
// Convert to the MAF AIAgent type to leverage polymorphism
AIAgent writer = new Microsoft.Agents.AI.ChatClientAgent(
    chatClient: cc_adapter, new Microsoft.Agents.AI.ChatClientAgentOptions()
    {
        Name = "Writer",
        Instructions = instructions
    });

// Now we can use the agent to run tasks
Microsoft.Agents.AI.AgentRunResponse response = await writer.RunAsync(question);
Console.WriteLine(response.Text);


// Create a specialized translator agent
// In C#, a variable (or parameter) typed as an abstract base class can hold an instance of any derived concrete class.
// Method calls on that variable use virtual dispatch to invoke the derived implementation (ChatClientAgent in your example).
AIAgent translator = new ChatClientAgent(
    cc_adapter,
    new ChatClientAgentOptions
    {
        Name = "translator",
        Instructions = "Translate the story to Italian."
    });

Console.WriteLine("\n\n+++ Running translator +++\n");

// This time we print the story with streaming
await foreach (var responseChunk in translator.RunStreamingAsync(response.Text))
{
    Console.Write(responseChunk.Text);
}

Console.WriteLine("\n\n+++ Running workflow +++\n");

// Create a workflow that connects writer to translator
Workflow workflow =
    AgentWorkflowBuilder
        .BuildSequential(writer, translator);

AIAgent workflowAgent = workflow.AsAgent();

await foreach (var responseChunk in workflowAgent.RunStreamingAsync(question))
{
    Console.Write(responseChunk.Text);
}


#region Tools as inline functions
[Description("Provides the author of the story.")]
string GetStoryAuthor() => "Mauro Minella";

[Description("Provides additional details of the story.")]
string FormatStory(string title, string author, string story) =>
    $"Story title: {title}\nAuthor: {author}\n\n{story}";

// convert simple functions to AIFunctions
Microsoft.Extensions.AI.AIFunction aiFunctionGetStoryAuthor = Microsoft.Extensions.AI.AIFunctionFactory.Create(GetStoryAuthor);
var aiFunctionFormatStory = AIFunctionFactory.Create(FormatStory); // same as above
#endregion


AIAgent writerWithTools = new Microsoft.Agents.AI.ChatClientAgent(
    chatClient: cc_adapter, new Microsoft.Agents.AI.ChatClientAgentOptions()
    {
        Name = "WriterWithTools",
        Instructions = instructionsToolsAware,
        ChatOptions = new Microsoft.Extensions.AI.ChatOptions
        {
            Tools = [
                aiFunctionGetStoryAuthor,
                aiFunctionFormatStory
            ],
        }
    });

Console.WriteLine("\n\n+++ Running workflow-with-tools +++\n");

// Create a workflow that connects the new writerWithTools agent to the translator agent
Microsoft.Agents.AI.Workflows.Workflow workflowWithTools =
    AgentWorkflowBuilder
        .BuildSequential(writerWithTools, translator);

AIAgent workflowAgentWithTools = workflowWithTools.AsAgent();

await foreach (var responseChunk in workflowAgentWithTools.RunStreamingAsync(question))
{
    Console.Write(responseChunk.Text);
}