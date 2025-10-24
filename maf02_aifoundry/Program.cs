using Azure.AI.Agents.Persistent;
using Microsoft.Agents.AI;

var clientEndpoint = Environment.GetEnvironmentVariable("AIF_STD_PROJECT_ENDPOINT"!);
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
const string JokerName = "Joker";
const string JokerInstructions = "You are good at telling jokes.";

// Get a client to create/retrieve server side agents with.
var persistentAgentsClient = new Azure.AI.Agents.Persistent.PersistentAgentsClient(
    clientEndpoint,
    new Azure.Identity.AzureCliCredential());


#region OPTION 1 TO USE SERVER SIDE PERSISTENT AGENTS
// You can create a server side persistent agent with the Azure.AI.Agents.Persistent SDK.
var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: deploymentName,
    name: JokerName,
    instructions: JokerInstructions);

// You can retrieve an already created server side persistent agent as an AIAgent.
AIAgent mafAgent1 = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);
#endregion

#region OPTION 2 TO USE SERVER SIDE PERSISTENT AGENTS
AIAgent mafAgent2 = await persistentAgentsClient.CreateAIAgentAsync(
    model: deploymentName,
    name: JokerName,
    instructions: JokerInstructions);
#endregion

// You can then invoke the agent like any other AIAgent.
Microsoft.Agents.AI.AgentThread thread = mafAgent1.GetNewThread();
Console.WriteLine(await mafAgent1.RunAsync("Tell me a joke about a pirate.", thread));

// Cleanup for sample purposes.
await persistentAgentsClient.Administration.DeleteAgentAsync(mafAgent1.Id);
await persistentAgentsClient.Administration.DeleteAgentAsync(mafAgent2.Id);