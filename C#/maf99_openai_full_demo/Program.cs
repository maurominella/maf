#region libraries and variables
using System.ClientModel;
using System.ComponentModel;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

var question = "Write a short story about a haunted house, using less than 50 words, in addition to possible details from the provided tools like author, title, and genre.";
var agent1_instructions = "Write stories that are engaging and creative ";
var agent1_instructionsToolsAware = agent1_instructions + " At the beginning of each story, please add all its possible details taken from the provided tools.";
var agent2_instructions = "You are a translator. Do not skip any part of the input text, even if it is repetitive looks redundant, or out of context. Always translate the **ENTIRE** input text into Italian immediately, without asking for confirmation or adding extra commentary. Output only the translated text.";

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_BAS_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Missing project endpoint env var");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

var mafOpenAIAgentName = "mafOpenAIAgent";
var foundryV1AgentName = "translatorAgent";

#endregion

#region openai client creation using ME-AI full package
// chat client creation using the OpenAI Extension (rather than the original OpenAI library)

var cc = new OpenAI.Chat.ChatClient("gpt-4o-mini",
 new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_MODELS_PAT_CLASSIC")!),
 new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") });

 // Send a chat request using the vendor-specific client
ChatCompletion response_openAI = (await cc.CompleteChatAsync("Hi there!")).Value;

Console.WriteLine("\n\n+++ Running mafOpenAIAgent with OpenAI client +++\n");
Console.WriteLine(response_openAI.Content[0].Text);
#endregion

#region MAF agent end-to-end implementation on top of the OpenAI chat client
// Adapt OpenAI ChatClient to IChatClient
IChatClient cc_adapter = cc.AsIChatClient();

// Create a writer agent that uses the chat client to generate a story
AIAgent mafOpenAIAgent = new ChatClientAgent(
    chatClient: cc_adapter,
    name: mafOpenAIAgentName,
    instructions: agent1_instructions
);

AgentResponse story = await mafOpenAIAgent.RunAsync(question);

Console.WriteLine("\n\n+++ Running mafOpenAIAgent with MAF client +++\n");
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
    name: mafOpenAIAgentName,
    instructions: agent1_instructionsToolsAware,
    tools: [
            aiFunctionGetStoryAuthor,
            aiFunctionFormatStory
        ]
);

// Invoke the agent WITH TOOLS (and streaming support).
Console.WriteLine("\n\n+++ Running mafOpenAIWithToolsAgent with MAF client, using Tools +++\n");
await foreach (var update in mafOpenAIWithToolsAgent.RunStreamingAsync(question))
{
    Console.Write(update);
}
#endregion

#region MAF agent end-to-end implementation on top of the Foundry V1 agent framework
// Create a Project client with Azure CLI Credential (or any other TokenCredential of your choice)
var aiFoundryPersistentProjectClient = new PersistentAgentsClient(projectEndpoint, new AzureCliCredential());

PersistentAgent metadataClassicFoundryAgent2 = await aiFoundryPersistentProjectClient.Administration.CreateAgentAsync(
    model: deploymentName, 
    name: foundryV1AgentName, 
    instructions: agent2_instructions);

AIAgent mafClassicFoundryAgent = await aiFoundryPersistentProjectClient.GetAIAgentAsync(metadataClassicFoundryAgent2.Id);

var mafAgentResponse = await mafClassicFoundryAgent.RunAsync("This is some text to translate");
Console.WriteLine("\n\n+++ Running translatorAgent alone with MAF client +++\n");
Console.WriteLine(mafAgentResponse.Text);
#endregion

#region Create a workflow agent that uses both the storyteller and translator agents
Workflow workflow = AgentWorkflowBuilder
    .BuildSequential(mafOpenAIWithToolsAgent, mafClassicFoundryAgent);


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
    .AddEdge(humanPort, mafClassicFoundryAgent)
    .WithOutputFrom(mafClassicFoundryAgent)
    .WithName("Storytelling and translation workflow")
    .WithDescription("A workflow that first generates a story using the mafOpenAIWithToolsAgent, and then translates it to Italian using the translatorAgent.");
*/

var workflowResponse = await workflowAgent.RunAsync(question);
var finalText = workflowResponse.Messages.LastOrDefault()?.Text ?? "[no text output from workflow]";

Console.WriteLine("\n\n+++ Running workflowAgent with MAF OpenAI <client with tools> + MAFFoundryV1 Client +++\n");
Console.WriteLine(finalText);
#endregion

Console.WriteLine("\nProgram has completed.");