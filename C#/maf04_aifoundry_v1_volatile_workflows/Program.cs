# region Libraries, Constants and Variables
using System.ComponentModel.DataAnnotations;
using Azure.AI.Agents;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_BAS_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Missing project endpoint env var");

var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

const string agent1Name = "WriterAgent01";
const string agent2Name = "TranslatorAgent01";

const string agent1Instructions = "You are good at telling jokes. You speak English only, even if the question is in another language.";
const string agent2Instructions = "You are a translator. Always translate the input text into Italian immediately, without asking for confirmation or adding extra commentary. Output only the translated text.";
#endregion

#region Create or Retrieve two server side V2 agents with the Azure.AI.Agents SDK client
// Create Persistent SDK AI Foundry project
var aiFoundryPersistentProjectClient = new PersistentAgentsClient(projectEndpoint, new AzureCliCredential());

PersistentAgent agent1 = await aiFoundryPersistentProjectClient.Administration.CreateAgentAsync(
    model: deploymentName, 
    name: agent1Name, 
    instructions: agent1Instructions);

PersistentAgent agent2 = await aiFoundryPersistentProjectClient.Administration.CreateAgentAsync(
    model: deploymentName, 
    name: agent2Name, 
    instructions: agent2Instructions);
#endregion

#region Create the first PERSISTENT threads (service-side) and add a message to the thread
// create the thread
PersistentAgentThread thread1 = (await aiFoundryPersistentProjectClient.Threads.CreateThreadAsync()).Value;

// add a message to the thread
await aiFoundryPersistentProjectClient.Messages.CreateMessageAsync(
    threadId: thread1.Id,
    role: MessageRole.User,
    content: "Tell me a joke about a dog."
);
#endregion

#region Run the agent on the thread and wait for completion
// retrieve the messages of the thread
var runResult = await aiFoundryPersistentProjectClient.Runs.CreateRunAsync(
    thread: thread1,
    agent: agent1
);

ThreadRun run;
do
{
    await Task.Delay(500);
    // [1](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.agents.persistent-readme?view=azure-dotnet)
    run = (await aiFoundryPersistentProjectClient.Runs.GetRunAsync(thread1.Id, runResult.Value.Id)).Value; 
} while (run.Status != Azure.AI.Agents.Persistent.RunStatus.Completed 
    && run.Status != Azure.AI.Agents.Persistent.RunStatus.Failed 
    && run.Status != Azure.AI.Agents.Persistent.RunStatus.Cancelled);
#endregion

#region Retrieve agent reply from the thread
// IMPORTANT: GetMessagesAsync returns AsyncPageable<PersistentThreadMessage> (not awaitable).
// Use 'await foreach' to iterate the async sequence, or materialize to a list if needed.
// This is the standard Azure SDK async pagination pattern. [1](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.agents.persistent-readme?view=azure-dotnet)

// Option A: iterate and pick the last agent message on the fly
PersistentThreadMessage? lastAgent = null;
await foreach (var m in aiFoundryPersistentProjectClient.Messages.GetMessagesAsync(thread1.Id))
{
    if (m.Role == MessageRole.Agent)
    {
        lastAgent = m;
    }
}

// Safely extract the first text block from ContentItems (if any)
var agentText = lastAgent?
    .ContentItems?
    .OfType<Azure.AI.Agents.Persistent.MessageTextContent>()
    .FirstOrDefault()?
    .Text;

var agent1TextSafe = agentText ?? "[no agent reply found]";

Console.WriteLine(agent1TextSafe);
#endregion


#region Create the second PERSISTENT thread (service-side) and add a message to the thread as the output of the first agent
// create the thread
PersistentAgentThread thread2 = (await aiFoundryPersistentProjectClient.Threads.CreateThreadAsync()).Value;

// add a message to the second thread
await aiFoundryPersistentProjectClient.Messages.CreateMessageAsync(
    threadId: thread2.Id,
    role: MessageRole.User,
    content: agent1TextSafe
);
#endregion

#region Run the second agent on the output of the first agent, and wait for completion
// retrieve the messages of the thread
var runResult2 = await aiFoundryPersistentProjectClient.Runs.CreateRunAsync(
    thread: thread2,
    agent: agent2
);

ThreadRun run2;
do
{
    await Task.Delay(500);
    // [1](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.agents.persistent-readme?view=azure-dotnet)
    run2 = (await aiFoundryPersistentProjectClient.Runs.GetRunAsync(thread2.Id, runResult2.Value.Id)).Value; 
} while (run2.Status != Azure.AI.Agents.Persistent.RunStatus.Completed 
    && run2.Status != Azure.AI.Agents.Persistent.RunStatus.Failed 
    && run2.Status != Azure.AI.Agents.Persistent.RunStatus.Cancelled);
#endregion

#region Retrieve agent reply from the thread
// IMPORTANT: GetMessagesAsync returns AsyncPageable<PersistentThreadMessage> (not awaitable).
// Use 'await foreach' to iterate the async sequence, or materialize to a list if needed.
// This is the standard Azure SDK async pagination pattern. [1](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.agents.persistent-readme?view=azure-dotnet)

// Option A: iterate and pick the last agent message on the fly
PersistentThreadMessage? lastAgent2 = null;
await foreach (var m in aiFoundryPersistentProjectClient.Messages.GetMessagesAsync(thread2.Id))
{
    if (m.Role == MessageRole.Agent)
    {
        lastAgent2 = m;
    }
}

// Safely extract the first text block from ContentItems (if any)
var agentText2 = lastAgent2?
    .ContentItems?
    .OfType<Azure.AI.Agents.Persistent.MessageTextContent>()
    .FirstOrDefault()?
    .Text;

var agent2TextSafe = agentText2 ?? "[no agent reply found]";

Console.WriteLine(agent2TextSafe);
#endregion


#region Create a volatile workflow - linear pipeline with the two agents
// Create workflow in Foundry. Please note that the workflow is NOT persisted in Foundry
// Build the workflow as a linear sequence of agents

ChatClientAgent writerAgent = await aiFoundryPersistentProjectClient.GetAIAgentAsync(agent1.Id);
ChatClientAgent translatorAgent  = await aiFoundryPersistentProjectClient.GetAIAgentAsync(agent2.Id);

Workflow workflow = AgentWorkflowBuilder
    .BuildSequential(writerAgent, translatorAgent);


AIAgent workflowAgent = workflow.AsAIAgent();

var finalText = await workflowAgent.RunAsync("Tell me a joke about a dog.");

Console.WriteLine("=== Final workflow output ===");
Console.WriteLine(finalText);


#endregion


/*
#region ====== ONE-LINER (Agent Framework) ====== >> method "AIAgent + Foundry V2"
var aiFoundryProjectClient = new AIProjectClient(new Uri(projectEndpoint), new AzureCliCredential());
// if you just want to LOAD an EXISTING agent, use:
// AIAgent agent1 = await aiFoundryProjectClient.GetAIAgentAsync(name: agent1Name);

// CreazioneFoundry V2 agent (just the first time, then you can load it with GetAIAgentAsync)
// This is NOT a MAF agent, but a Foundry V2 agent that can be used as a MAF agent and also directly with the Persistent SDK clients
await aiFoundryProjectClient.CreateAIAgentAsync(
    model: deploymentName,
    name: agent1Name,
    instructions: agent1Instructions);

// Now you can retrieve the agent with GetAIAgentAsync or use the returned object from CreateAIAgentAsync
// This is a Foundry V2 agent that can be used as a MAF agent and also directly with the Persistent SDK clients
AFAI.AIAgent agent1 = await aiFoundryProjectClient.GetAIAgentAsync(name: agent1Name);


var r1 = await agent1.RunAsync("Tell me a joke");
Console.WriteLine(r1.Text);
var r2 = await agent1.RunAsync("Translate to Italian");
Console.WriteLine(r2.Text);
var r3 = await agent1.RunAsync("Make it longer");
Console.WriteLine(r3.Text);

// AFAI.AgentThread thread = new AFAI.AgentThread();

#endregion



/*



#region PersistedAgentsClient --> Create client, thread and send message to thread 
// This is the main entry point for working with server side agents in Foundry. 
// With this client you can create and retrieve agents, threads and messages that are persisted in Foundry. 
// You can also use this client to run agents and workflows on the server side, with the ability to stream the output.


// Get a client to create/retrieve server side agents with.
var aiFoundryPersistentProjectClient = new PersistentAgentsClient(projectEndpoint, new AzureCliCredential());
PersistentAgentThread threadAgent1 = await aiFoundryPersistentProjectClient.Threads.CreateThreadAsync();
Console.WriteLine($"Created thread #1 with id: {threadAgent1.Id}");
PersistentAgentThread threadAgent2 = await aiFoundryPersistentProjectClient.Threads.CreateThreadAsync();
Console.WriteLine($"Created thread with id: {threadAgent2.Id}");

await aiFoundryPersistentProjectClient.Messages.CreateMessageAsync(threadAgent1.Id, MessageRole.User, "Tell me a joke about a dog.");

#endregion

#region Create or Retrieve agents
// Create two server side V2 agents with the Azure.AI.Agents SDK client

// PersistentAgent agent = await aiFoundryPersistentProjectClient.Administration.GetAgentAsync("<agent-id>");

PersistentAgent agent1 = await aiFoundryPersistentProjectClient.Administration.CreateAgentAsync(
    model: deploymentName, 
    name: agent1Name, 
    instructions: agent1Instructions);

PersistentAgent agent2 = await aiFoundryPersistentProjectClient.Administration.CreateAgentAsync(
    model: deploymentName, 
    name: agent2Name, 
    instructions: agent2Instructions);
#endregion


#region Invoke agents

var run = await aiFoundryPersistentProjectClient.Runs.CreateRunAsync(threadAgent1, agent1);



#endregion

*/
/*
AgentThread threadAgent1 = agent1.GetNewThread();
var answerAgent1 = await agent1.RunAsync(
   "Tell me a joke about a pirate.", threadAgent1);

Console.WriteLine(answerAgent1.Text);

AgentThread threadAgent2 = agent2.GetNewThread();
Console.WriteLine(await agent2.RunAsync(
   answerAgent1.Text, threadAgent2));
#endregion

#region Create a volatile workflow - linear pipeline with the two agents
// Create workflow in Foundry. Please note that the workflow is NOT persisted in Foundry
// Build the workflow as a linear sequence of agents
Workflow volatileWorkflow1 =
    AgentWorkflowBuilder
        .BuildSequential(agent1, agent2);

AIAgent workflowAgent = volatileWorkflow1.AsAgent();

AgentThread threadwfAgent = workflowAgent.GetNewThread();
Console.WriteLine(await workflowAgent.RunAsync(
   "Tell me a joke about a dog.", threadwfAgent));
#endregion


#region Create a volatile workflow - explicit and composable API, ideal for complex workflows and higher control
// Create workflow in Foundry. Please note that the workflow is NOT persisted in Foundry
// Build the workflow by adding executors and connecting them
var volatileWorkflow2 = new WorkflowBuilder(agent1)
    .AddEdge(agent1, agent2)
    .Build();

// Streaming execution (TurnToken pattern)
await using var run = await InProcessExecution.StreamAsync(
    volatileWorkflow2, new ChatMessage(ChatRole.User, "Tell me a joke about a pirate."));
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is AgentRunUpdateEvent update)
    {
        Console.Write($"{update.Data}");
    }
}
#endregion

#region Teardown
// ask to press Y/N to delete the created agents
Console.Write("Do you want to delete the created agents (Y/N)? > ");
var input = Console.Read();
if (char.ToUpper((char)input) == 'Y')
{
    aiFoundryProjectClient.Agents.DeleteAgent(agentName:agent1.Name);
    Console.WriteLine($"Deleted agent: {agent1.Name}");
    aiFoundryProjectClient.Agents.DeleteAgent(agentName:agent2.Name);
    Console.WriteLine($"Deleted agent: {agent2.Name}");
}
else
{
    Console.WriteLine("Agents not deleted.");
    return;
}
#endregion

Console.WriteLine("Program has ended successfully.");

*/

Console.WriteLine("Program has ended successfully.");