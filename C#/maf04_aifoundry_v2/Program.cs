// inspired by https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/GettingStarted/AgentProviders/Agent_With_AzureAIProject/Program.cs

// make sure that the following environment variables are defined, using PowerShell or CMD with Administator privileges:
// setx AIF_BASPROJECT_ENDPOINT "https://aif1bassvj36b.services.ai.azure.com/api/projects/aif1basswcprj01" # afer this, please restart the terminal
// setx AIF_STD_PROJECT_ENDPOINT "https://aif2stdsvhdu2.services.ai.azure.com/api/projects/aif2stdwusprj01hdu2" # afer this, please restart the terminal
// $env:AZURE_OPENAI_CHAT_DEPLOYMENT_NAME = "gpt-4o" # to make it immediate in the current terminal session
// check with echo $env:AIF_BASPROJECT_ENDPOINT (after restarting the terminal) that the variables are set correctly.

# region Constants and Variables
using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Microsoft.Agents.AI;
using Azure.Identity;

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_BASPROJECT_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

const string agent1V2Name = "jokeragent1v2";
const string agent2V2Name = "jokeragentnewv2";

const string agentInstructionsV2 = "You are good at telling jokes. You speak English only, even if the question is in another language.";
#endregion

// Get a client to create/retrieve server side agents with.
var aiFoundryProjectClient = new AIProjectClient(
    new Uri(projectEndpoint!), 
    new Azure.Identity.AzureCliCredential());

#region Create agent #1 V2
// Create a server side V2 agent version with the Azure.AI.Agents SDK client below. 
AIAgent agent1V2 = await aiFoundryProjectClient.CreateAIAgentAsync(
    model: deploymentName, 
    name: agent1V2Name, 
    instructions: agentInstructionsV2);
#endregion

#region Retrieve agent #2 V2
// Retrieve  a server side V2 agent version with the Azure.AI.Agents SDK client below. 
AIAgent agent2V2 = await aiFoundryProjectClient.GetAIAgentAsync(name: agent2V2Name);
#endregion


#region Use agent #1 V2
Console.WriteLine($"Using agent: {agent1V2.Name}");
// Get the AIAgent latest version just providing its name.
// The AIAgent version can be accessed via the GetService method.
var latestVersionAgent1 = agent1V2.GetService<Azure.AI.Projects.OpenAI.AgentVersion>()!;
Console.WriteLine($"Agent version: {latestVersionAgent1.Version}");

// Once you have the AIAgent, you can invoke it like any other AIAgent.
AgentThread threadAgent1V2 = agent1V2.GetNewThread();
Console.WriteLine(await agent1V2.RunAsync("Tell me a joke about a pirate.", threadAgent1V2));

// This will use the same thread to continue the conversation.
Console.WriteLine(await agent1V2.RunAsync("Now tell me a joke about a cat and a dog using last joke as the anchor.", threadAgent1V2));

// Cleanup by agent name removes both agent versions created (jokerAgentV1 + jokerAgentV2).
aiFoundryProjectClient.Agents.DeleteAgent(agentName:agent1V2.Name);
Console.WriteLine($"Deleted agent: {agent1V2.Name}");
#endregion


#region Use agent #2 V2
Console.WriteLine($"Using agent: {agent2V2.Name}");
// Get the AIAgent latest version just providing its name.
// The AIAgent version can be accessed via the GetService method.
var latestVersionAgent2 = agent2V2.GetService<Azure.AI.Projects.OpenAI.AgentVersion>()!;
Console.WriteLine($"Agent version: {latestVersionAgent2.Version}");

// Once you have the AIAgent, you can invoke it like any other AIAgent.
AgentThread threadAgent2V2 = agent2V2.GetNewThread();
Console.WriteLine(await agent2V2.RunAsync("Tell me a joke about a pirate.", threadAgent2V2));

// This will use the same thread to continue the conversation.
Console.WriteLine(await agent2V2.RunAsync("Now tell me a joke about a cat and a dog using last joke as the anchor.", threadAgent1V2));

// Cleanup by agent name removes both agent versions created (jokerAgentV1 + jokerAgentV2).
// aiFoundryProjectClient.Agents.DeleteAgent(agentName:agent2V2.Name);
Console.WriteLine($"Deleted agent: {agent2V2.Name}");
#endregion

Console.WriteLine("Done.");