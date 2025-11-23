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

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_BASPROJECT_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
const string newAgentNameV1 = "jokeragentnewv1"; // jokeragentnewv1
const string newAgentNameV2 = "jokeragentnewv2"; // jokeragentnewv2
const string existingAgentNameV1 = "jokeragentv1"; // jokeragentv1
const string existingAgentNameV2 = ""; // jokeragentv2
const string agentInstructionsV1 = "You are good at telling jokes. You speak Italian only, even if the question is in another language.";
const string agentInstructionsV2 = "You are good at telling jokes. You speak English only, even if the question is in another language.";

AIAgent myAgentV1;
AIAgent myAgentV2;
#endregion

# region Project Settings, assuming all agents are in the same project
// Get a client to create/retrieve server side agents with.
var aiFoundryProjectClient = new Azure.AI.Projects.AIProjectClient(
    new Uri(projectEndpoint!), new Azure.Identity.AzureCliCredential());
#endregion



# region Agents V2
if (!string.IsNullOrEmpty(existingAgentNameV2)) // if existingAgentName is set, retrieve the existing agent version instead of creating a new one.
{
    // Get the AIAgent latest version just providing its name.

    AIAgent jokerAgentLatest = aiFoundryProjectClient.GetAIAgent(name: existingAgentNameV2);

    myAgentV2 = await aiFoundryProjectClient.GetAIAgentAsync(name: existingAgentNameV2);   
}
else // Create a server side agent version with the Azure.AI.Agents SDK client below. 
{
    // Create a new AIAgent version (V2) by providing the same name with a different definition.
    myAgentV2 = aiFoundryProjectClient.CreateAIAgent(name: newAgentNameV2, model: deploymentName, instructions: agentInstructionsV2 + "V2");
}

// Get the AIAgent latest version just providing its name.
// The AIAgent version can be accessed via the GetService method.
var latestVersion = myAgentV2.GetService<Azure.AI.Projects.OpenAI.AgentVersion>()!;
Console.WriteLine($"Agent version: {latestVersion.Version}");

// Once you have the AIAgent, you can invoke it like any other AIAgent.
AgentThread threadV2 = myAgentV2.GetNewThread();
Console.WriteLine(await myAgentV2.RunAsync("Tell me a joke about a pirate.", threadV2));

// This will use the same thread to continue the conversation.
Console.WriteLine(await myAgentV2.RunAsync("Now tell me a joke about a cat and a dog using last joke as the anchor.", threadV2));

// Cleanup by agent name removes both agent versions created (jokerAgentV1 + jokerAgentV2).
// aiFoundryProjectClient.Agents.DeleteAgent(agentName:myAgentV2.Name);
Console.WriteLine($"Deleted agent: {myAgentV2.Name}");
#endregion

# region Agents V1

Azure.AI.Agents.Persistent.AzureAIAgentsPersistentContext myAgentPersistentV1;

;

if (!string.IsNullOrEmpty(existingAgentNameV1)) // if existingAgentNameV1 is set, retrieve the existing agent version instead of creating a new one.
{
    // You can retrieve an AIAgent for a already created server side agent version.
    myAgentV1 = aiFoundryProjectClient.GetAIAgent(name: existingAgentNameV1);
}
else // Create a new AI Foundry V1 Agent
{
    // Define the agent you want to create. (Prompt Agent in this case)
    var agentVersionCreationOptionsV1 = new AgentVersionCreationOptions(
        new Azure.AI.Projects.OpenAI.PromptAgentDefinition(model: deploymentName) { Instructions = agentInstructionsV1 });

    // Azure.AI.Agents SDK creates and manages agent by name and versions.
    var agentVersionV1 = aiFoundryProjectClient.Agents.CreateAgentVersion(agentName: newAgentNameV1, options: agentVersionCreationOptionsV1);

    // Note:
    //      agentVersion.Id = "<agentName>:<versionNumber>",
    //      agentVersion.Version = <versionNumber>,
    //      agentVersion.Name = <agentName>

    // Create a new AIAgent version (V1) by providing a name and definition.
    myAgentV1 = aiFoundryProjectClient.CreateAIAgent(name: newAgentNameV1, model: deploymentName, instructions: agentInstructionsV1);
}

// Once you have the AIAgent, you can invoke it like any other AIAgent.
AgentThread threadV1 = myAgentV1.GetNewThread();
Console.WriteLine(await myAgentV1.RunAsync("Tell me a joke about a pirate.", threadV1));
# endregion