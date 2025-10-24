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

// You can create a server side persistent agent with the Azure.AI.Agents.Persistent SDK.
var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: deploymentName,
    name: JokerName,
    instructions: JokerInstructions);


// You can retrieve an already created server side persistent agent as an AIAgent.
AIAgent mafAgent = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);

Console.ReadLine();