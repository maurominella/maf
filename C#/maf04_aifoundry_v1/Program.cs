// inspired by https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-types/azure-ai-foundry-agent?pivots=programming-language-csharp

// make sure that the following environment variables are defined, using PowerShell or CMD with Administator privileges:
// setx AIF_BASPROJECT_ENDPOINT "https://aif1bassvj36b.services.ai.azure.com/api/projects/aif1basswcprj01" # afer this, please restart the terminal
// $env:AZURE_OPENAI_CHAT_DEPLOYMENT_NAME = "gpt-4o" # to make it immediate in the current terminal session
// check with echo $env:AIF_BASPROJECT_ENDPOINT (after restarting the terminal) that the variables are set correctly.

# region Constants and Variables
using Azure.AI.Agents.Persistent;
using Microsoft.Agents.AI;
using Azure.Identity;

const string agent1V1Name = "jokeragent1v1"; 
const string agent2V2Name = "jokeragent2v2"; 
const string agentInstructionsV1 = "You are good at telling jokes. You speak Italian only, even if the question is in another language.";
var projectEndpoint = Environment.GetEnvironmentVariable("AIF_BASPROJECT_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
#endregion

// Create PersistentAgentsClient to the AI Foundry project
var persistentAgentsClient = new PersistentAgentsClient(
    projectEndpoint,
    new AzureCliCredential());

#region Create persistent agent #1 V1 - in two steps
// First, create the agent in AI Foundry as a PersistentAgent object
var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: deploymentName,
    name: agent1V1Name,
    instructions: agentInstructionsV1);

// Secondly, retrieve as an AIAgent the agent that was just created using its ID
AIAgent agent1 = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);
#endregion

#region Create persistent agent #2 V1 in a single step
// Create the AI Foundry agent as an AIAgent object directly
AIAgent agent2 = await persistentAgentsClient.CreateAIAgentAsync(
    model: deploymentName,
    name: agent2V2Name,
    instructions: agentInstructionsV1);
#endregion

#region Retrieve the agent #3 V1 from agent #2 V1 as an AIAgent using its ID
AIAgent agent3 = await persistentAgentsClient.GetAIAgentAsync(agent2.Id);
#endregion

// Invoke the agent and output the text result.
Console.WriteLine(await agent3.RunAsync("Tell me a joke about a pirate."));