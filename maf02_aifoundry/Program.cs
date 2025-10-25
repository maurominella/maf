using Azure.AI.Agents.Persistent;
using Microsoft.Agents.AI;

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_STD_PROJECT_ENDPOINT"!);
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
const string JokerName = "Joker";
const string JokerInstructions = "You are good at telling jokes.";

// Get a client to create/retrieve server side agents with.
var aiFoundryAgentsClient = new Azure.AI.Agents.Persistent.PersistentAgentsClient(
    projectEndpoint,
    new Azure.Identity.AzureCliCredential());

#region OPTION 1 TO USE SERVER SIDE PERSISTENT AGENTS
// You can create a server side persistent agent with the Azure.AI.Agents.Persistent SDK.
// The following instruction creates the AI Foundry Agent in the AI Foundry project and returns its metadata.
var aiFoundryAgent1Metadata = await aiFoundryAgentsClient.Administration.CreateAgentAsync(
    model: deploymentName,
    name: JokerName,
    instructions: JokerInstructions);

// Here we extract the created agent from its metadata, but it's still NOT a MAF agent yet.
var aiFoundryAgent1 = aiFoundryAgent1Metadata.Value;

// You can retrieve an already created server side persistent agent as an AIAgent.
// This allows you to separate the creation and usage of the agent.
// The returned object is ChatClientAgent, but here it's typed as the base AIAgent class.
// Here we convert the persistent AI Foundry agent to a MAF agent.
var aiFoundryMafAgent1 = await aiFoundryAgentsClient.GetAIAgentAsync(aiFoundryAgent1.Id);
#endregion

#region OPTION 2 TO USE SERVER SIDE PERSISTENT AGENTS
AIAgent aiFoundryMafAgent2 = await aiFoundryAgentsClient.CreateAIAgentAsync(
    model: deploymentName,
    name: JokerName,
    instructions: JokerInstructions);
#endregion

// You can then invoke the agent like any other AIAgent.
Microsoft.Agents.AI.AgentThread thread = aiFoundryMafAgent1.GetNewThread();
Console.WriteLine(await aiFoundryMafAgent1.RunAsync("Tell me a joke about a pirate.", thread));

// Cleanup for sample purposes.
await aiFoundryAgentsClient.Administration.DeleteAgentAsync(aiFoundryMafAgent1.Id);
await aiFoundryAgentsClient.Administration.DeleteAgentAsync(aiFoundryMafAgent2.Id);