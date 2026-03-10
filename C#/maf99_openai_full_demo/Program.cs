#region libraries and variables
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using Azure.AI.Projects;
using Microsoft.Agents.AI.Workflows;

var question = "Write a short story about a haunted house, using less than 50 words.";
var agent1_instructions = "Write stories that are engaging and creative ";
var agent1_instructionsToolsAware = agent1_instructions + " At the beginning of each story, please add all its possible details taken from the provided tools.";
var agent2_instructions = "You are a translator. Do not skip any part of the input text, even if it is repetitive looks redundant, or out of context. Always translate the **ENTIRE** input text into Italian immediately, without asking for confirmation or adding extra commentary. Output only the translated text.";

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_STD_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Missing project endpoint env var");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

var openAIAgentName    = "openAIStorytellerAgent";
var foundryV2AgentName = "foundryTranslatorAgent";

#endregion

#region OpenAI client creation using "pure" OpenAI library and invocation
// chat client creation and invokation using the OpenAI SDK (that installs the original OpenAI library)
var cc = new OpenAI.Chat.ChatClient("gpt-4o-mini",
 new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_MODELS_PAT_CLASSIC")!),
 new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") });

 // Send a chat request using the vendor-specific client
ChatCompletion response_openAI = (await cc.CompleteChatAsync("Hi there!")).Value;

Console.WriteLine("\n\n+++ Running openAIAgent with OpenAI client +++\n");
Console.WriteLine(response_openAI.Content[0].Text);
#endregion

#region Full MAF agent to create OpenAI chat client as a MAF-compatible chat client
// Adapt OpenAI ChatClient to IChatClient
IChatClient cc_adapter = cc.AsIChatClient();

// Create a writer agent that uses the chat client to generate a story
AIAgent mafOpenAIAgent = new ChatClientAgent(
    chatClient: cc_adapter,
    name: openAIAgentName,
    instructions: agent1_instructions
);
Console.WriteLine("\n\n+++ Same question, using the MAF Agent +++\n");
Console.WriteLine(await mafOpenAIAgent.RunAsync("Hi there!"));

AgentResponse story = await mafOpenAIAgent.RunAsync(question);
Console.WriteLine("\n\n+++ Invoking  MAF Agent for a more complex question +++\n");
Console.WriteLine(story.Text);

//region Tools as inline functions
[Description("Provides the author of the story.")]
string GetStoryAuthor() => "Mauro Minella";

[Description("Provides the the story with title and author.")]
string FormatStory(string title, string author, string story) =>
    $"Story title: {title}\nAuthor: {author}\n\n{story}";


// convert simple functions to AIFunctions
AIFunction aiFunctionGetStoryAuthor = AIFunctionFactory.Create(GetStoryAuthor);
var aiFunctionFormatStory = AIFunctionFactory.Create(FormatStory);

AIAgent mafOpenAIWithToolsAgent = new ChatClientAgent(
    chatClient: cc_adapter,
    name: openAIAgentName,
    instructions: agent1_instructionsToolsAware,
    tools: [
            aiFunctionGetStoryAuthor,
            aiFunctionFormatStory
        ]
);

// Invoke the agent WITH TOOLS (and streaming support).
Console.WriteLine("\n\n+++ Invoking mafOpenAIAgent, using Tools +++\n");
await foreach (var update in mafOpenAIWithToolsAgent.RunStreamingAsync(question))
{
    Console.Write(update);
}
#endregion

#region Foundry V2 Prompt agents using MAF
// Leverage Azure CLI Credential (or any other TokenCredential of your choice)

var aiFoundryV2ProjectClient = new AIProjectClient(new Uri(projectEndpoint), new AzureCliCredential());

var mafPromptFoundryAgentV2 = await aiFoundryV2ProjectClient.CreateAIAgentAsync(
    name: openAIAgentName,
    model: deploymentName,
    instructions: agent2_instructions);


var mafAgentResponse = await mafPromptFoundryAgentV2.RunAsync("This is some text to translate");
Console.WriteLine("\n\n+++ Running Foundry Translator Agent alone with MAF client +++\n");
Console.WriteLine(mafAgentResponse.Text);
#endregion

#region Create a workflow agent that uses both the storyteller and translator agents
Workflow workflow = AgentWorkflowBuilder
    .BuildSequential(mafOpenAIWithToolsAgent, mafPromptFoundryAgentV2);

var builder = new WorkflowBuilder(mafOpenAIWithToolsAgent);

AIAgent workflowAgent = workflow.AsAIAgent();

/*
// This is a possible alternative way to create the workflow agent, using the WorkflowBuilder directly, 
// rather than first creating a Workflow and then converting it to an agent. The resulting workflowAgent would be the same in both cases.
AIAgent workflowAgent = builder.Build().AsAIAgent();

// Optional step to allow human input in the workflow, in this case we use it to pass the original question to the second agent, 
// but it could be used for other purposes as well, like allowing human feedback at different stages of the workflow.
var humanPort = RequestPort.Create<string, string>("human-input"); 

builder
    .AddEdge(mafOpenAIWithToolsAgent, humanPort)
    .AddEdge(humanPort, mafPromptFoundryAgentV2)
    .WithOutputFrom(mafPromptFoundryAgentV2)
    .WithName("Storytelling and translation workflow")
    .WithDescription("A workflow that first generates a story using the mafOpenAIWithToolsAgent, and then translates it to Italian using the translatorAgent.");
*/

var workflowResponse = await workflowAgent.RunAsync(question);
var finalText = workflowResponse.Messages.LastOrDefault()?.Text ?? "[no text output from workflow]";

Console.WriteLine("\n\n+++ Running workflowAgent with MAF OpenAI <client with tools>Agent + MAF FoundryV2 Agent +++\n");
Console.WriteLine(finalText);
#endregion

Console.WriteLine("\nProgram has completed.");