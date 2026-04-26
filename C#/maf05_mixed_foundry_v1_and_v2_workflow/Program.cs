# region Libraries, Constants and Variables
using System.ComponentModel.DataAnnotations;
using Azure.AI.Agents;
using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_STD_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Missing project endpoint env var");

var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

const string agent1Name = "WriterAgent01";
const string agent2Name = "TranslatorAgent01";

const string agent1Instructions = "You are good at telling jokes. You speak English only, even if the question is in another language.";
const string agent2Instructions = "You are a translator. Always translate the input text into Italian immediately, without asking for confirmation or adding extra commentary. Output only the translated text.";
#endregion

#region Create V1 and V2 Project clients with Azure CLI Credential (or any other TokenCredential of your choice)
// even if the statement is PersistentAgentsClient, we are actually creating a client for the project, 
// which is the main entry point to interact with Foundry. The same client will be used for both persistent and volatile scenarios, 
// as well as for administration and runtime operations.
var aiFoundryV1ClassicProjectClient = new PersistentAgentsClient(projectEndpoint, new AzureCliCredential());
var aiFoundryV2ProjectClient         = new AIProjectClient(new Uri(projectEndpoint), new AzureCliCredential());
#endregion

#region Create or Retrieve one server side V1/Classic agent METADATA with the Azure.AI.Agents SDK client
// we call them "metadataClassicFoundryAgent" because they contain metadata about the agents created in Foundry,
// they are actually created in Foundry with the instructions provided, but htey are not yet MAF agents,
// and they are not directly usable for chatting or workflows. We will convert them to MAF agents in a later step to show how to use them in a simpler way.

PersistentAgent metadataClassicFoundryAgent1 = await aiFoundryV1ClassicProjectClient.Administration.CreateAgentAsync(
    model: deploymentName, 
    name: agent1Name, 
    instructions: agent1Instructions);

#endregion

#region Create a PERSISTENT (=V1) thread for the first agent (service-side) and add a message to the thread
// create the thread
PersistentAgentThread thread1 = (await aiFoundryV1ClassicProjectClient.Threads.CreateThreadAsync()).Value;

// add a message to the thread
await aiFoundryV1ClassicProjectClient.Messages.CreateMessageAsync(
    threadId: thread1.Id,
    role: MessageRole.User,
    content: "Tell me a joke about a dog."
);
#endregion

#region Invoke the first V1 agent on the thread and wait for Run completion
// retrieve the messages of the thread
var runResult = await aiFoundryV1ClassicProjectClient.Runs.CreateRunAsync(
    thread: thread1,
    agent: metadataClassicFoundryAgent1
);

ThreadRun run;
do
{
    await Task.Delay(500);
    run = (await aiFoundryV1ClassicProjectClient.Runs.GetRunAsync(thread1.Id, runResult.Value.Id)).Value; 
} while (run.Status != Azure.AI.Agents.Persistent.RunStatus.Completed 
    && run.Status != Azure.AI.Agents.Persistent.RunStatus.Failed 
    && run.Status != Azure.AI.Agents.Persistent.RunStatus.Cancelled);
#endregion

#region Retrieve the first V1 agent reply from the thread
// Option A: iterate and pick the last agent message on the fly
PersistentThreadMessage? lastMessage = null;
await foreach (var m in aiFoundryV1ClassicProjectClient.Messages.GetMessagesAsync(thread1.Id))
{
    if (m.Role == MessageRole.Agent)
    {
        lastMessage = m;
    }
}

// Safely extract the first text block from ContentItems (if any)
var agentText = lastMessage?
    .ContentItems?
    .OfType<Azure.AI.Agents.Persistent.MessageTextContent>()
    .FirstOrDefault()?
    .Text;

var agent1TextSafe = agentText ?? "[no agent reply found]";

Console.WriteLine("\n\n=== Agent V1 Response with Foundry SDK ===");
Console.WriteLine(agent1TextSafe);
#endregion

#region For the second agent (FOUNDRY V2), we create it as a MAF agent in a one-shot way

// a longer way would be to use aiFoundryV2ProjectClient.Agents.CreateAgentVersionAsync
// if the Prompt agent exists, we can simply run agentv2 = await _aiProjectClient.GetAIAgentAsync("foundryagent-V2-with-bing");

// but here we want to show how to create it in a one-shot way, which is more convenient for quick testing and prototyping, 
// without the need to go through multiple steps of creating the agent version, then the agent, etc.
var mafPromptFoundryAgentV2 = await aiFoundryV2ProjectClient.CreateAIAgentAsync(
    name: agent2Name,
    model:deploymentName,
    instructions: agent2Instructions);

#endregion

#region Invoke the second agent and output the text result
// withouth memory reuse
await mafPromptFoundryAgentV2.RunAsync("What is the weather in Milan today?").ContinueWith(r =>
{
    Console.WriteLine("\n\n=== Agent V2 Response ===");
    Console.WriteLine(r.Result);
});

// with memory reuse - the agent can access the previous conversation history and context through the "session" object, 
// which is linked to the thread we created for the first agent. This allows us to have a continuous conversation across multiple agents, 
// and to keep the context alive across runs.
// Unfortunately, this is currently not possible to do with the current RC2/preview version of the Azure.AI.Agents SDK, where AgentSession is an abstract class
// var session = new AgentSession(thread.Value.Id);
// var r1 = await mafClassicFoundryAgent2.RunAsync("Tell me a joke about a pirate.", session: session);
#endregion

#region Create a volatile workflow - linear pipeline with the two agents
// The first agent is still a "pure" foundry agent, so we need to convert it to a MAF agent as well 
// to be able to use it in the workflow, which is a MAF construct.
AIAgent mafClassicFoundryAgentV1 = await aiFoundryV1ClassicProjectClient.GetAIAgentAsync(metadataClassicFoundryAgent1.Id);

Workflow workflow = AgentWorkflowBuilder
    .BuildSequential(mafClassicFoundryAgentV1, mafPromptFoundryAgentV2);

AIAgent workflowAgent = workflow.AsAIAgent();

var workflowResponse = await workflowAgent.RunAsync("Tell me a joke about a dog.");

var finalText = workflowResponse.Messages.LastOrDefault()?.Text ?? "[no text output from workflow]";

Console.WriteLine("\n\n=== Final workflow output ===");
Console.WriteLine(finalText);
#endregion

Console.WriteLine("\n\n=== Program has ended successfully ===");