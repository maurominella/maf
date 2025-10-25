// SUMMARY:
// - MAF: whatever is under Microsoft.Agents.AI (Agent, Options, RunAsync, AgentRunResponse)
// - Abstraction layer: Microsoft.Extensions.AI (interfaces, e.g. IChatClient)
// - Adapter OpenAI -> IChatClient: provided by the Microsoft.Extensions.AI.OpenAI package
// - SDK vendor: OpenAI package

// SCHEMA:
// OpenAI SDK → creates the concrete client (ChatClient).
// Microsoft.Extensions.AI.OpenAI → provides the adapter (AsIChatClient()).
// Microsoft.Extensions.AI → defines the interface (IChatClient).
// Microsoft.Agents.AI (MAF) → uses the interface to create and manage agents.


// - VENDOR SDK (NOT MAF PACKAGE), coming from NuGet package OpenAI.
// - Here we create the vendor Native Client (OpenAI.Chat.ChatClient) to OpenAI
//   needed to talk to an OpenAI-compatible endpoint (GitHub AI in this case).
// - This is NOT MAF code, but vendor-specific code, a bridge to the vendor SDK and LLM.
using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

var question = "Write a short story about a haunted house, using no more than 10 words.";

var cc = new OpenAI.Chat.ChatClient(
            "gpt-4o-mini",
            new System.ClientModel.ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN")!),
            new OpenAI.OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") });

// - The ADAPTER PROVIDER (from package Microsoft.Extensions.AI.OpenAI) exposes the extension AsIChatClient()
//   that builds a wrapper around the vendor-specific ChatClient (cc) to expose the standard IChatClient interface.
// - cc_adapter implements IChatClient interface through the adapter --> a concrete object that delegates all calls to cc.
// - The namespace does not always match the NuGet package name:
//   - AsIChatClient(...) is provided by the NuGet package Microsoft.Extensions.AI.OpenAI, even though 
//   - the class/method may live in the namespace Microsoft.Extensions.AI or in a static class called OpenAIClientExtensions.
// - This is needed to use MAF that does not know OpenAI, it only knows the standard IChatClient interface 
//   defined in Microsoft.Extensions.AI namespace of the Microsoft.Extensions.AI.OpenAI package.
Microsoft.Extensions.AI.IChatClient cc_adapter = Microsoft.Extensions.AI.OpenAIClientExtensions.AsIChatClient(cc);


// +++  FROM HERE ON, WE USE MAF PACKAGE ONLY  +++

Console.WriteLine("\n\n+++ Running writer +++\n");

// We start creating ChatClientAgent, which is a general-purpose agent that can talk to any IChatClient implementation.
AIAgent writer = new Microsoft.Agents.AI.ChatClientAgent(
    chatClient: cc_adapter, new Microsoft.Agents.AI.ChatClientAgentOptions()
    {
        Name = "Writer",
        Instructions = "Write stories that are engaging and creative."
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

AIAgent workflowAgent = await workflow.AsAgentAsync();

await foreach (var responseChunk in workflowAgent.RunStreamingAsync(question))
{
    Console.Write(responseChunk.Text);
}


[Description("Returns the author of the story.")]
string GetStoryAuthor() => "Mauro Minella";

[Description("Formats the story for display.")]
string FormatStory(string title, string author, string story) =>
    $"Story title: {title}\nAuthor: {author}\n\n{story}";


// We start creating ChatClientAgent, which is a general-purpose agent that can talk to any IChatClient implementation.
AIAgent writerWithTools = new Microsoft.Agents.AI.ChatClientAgent(
    chatClient: cc_adapter, new Microsoft.Agents.AI.ChatClientAgentOptions()
    {
        Name = "WriterWithTools",
        Instructions = question + ", using also available tools.",
        ChatOptions = new Microsoft.Extensions.AI.ChatOptions
        {
            Tools = [
                AIFunctionFactory.Create(GetStoryAuthor),
                AIFunctionFactory.Create(FormatStory)
            ],
        }
    });

Console.WriteLine("\n\n+++ Running workflow-with-tools +++\n");

// Create a workflow that connects the new writerWithTools agent to the translator agent
Workflow workflowWithTools =
    AgentWorkflowBuilder
        .BuildSequential(writerWithTools, translator);

AIAgent workflowAgentWithTools = await workflowWithTools.AsAgentAsync();

await foreach (var responseChunk in workflowAgentWithTools.RunStreamingAsync(question))
{
    Console.Write(responseChunk.Text);
}