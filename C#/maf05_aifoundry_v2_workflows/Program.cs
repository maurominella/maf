# region Constants and Variables
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Azure.AI.Projects;
using Microsoft.Agents.AI.Workflows;

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_BASPROJECT_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

const string agent1Name = "WriterAgent";
const string agent2Name = "TranslatorAgent";

const string agent1Instructions = "You are good at telling jokes. You speak English only, even if the question is in another language.";
const string agent2Instructions = "You are a translator. Always translate the input text into Italian immediately, without asking for confirmation or adding extra commentary. Output only the translated text.";
#endregion

// Get a client to create/retrieve server side agents with.
var aiFoundryProjectClient = new AIProjectClient(
    new Uri(projectEndpoint), 
    new Azure.Identity.AzureCliCredential());

#region Create agents
// Create two server side V2 agents with the Azure.AI.Agents SDK client 
AIAgent agent1 = await aiFoundryProjectClient.CreateAIAgentAsync(
    model: deploymentName, 
    name: agent1Name, 
    instructions: agent1Instructions);

AIAgent agent2 = await aiFoundryProjectClient.CreateAIAgentAsync(
    model: deploymentName, 
    name: agent2Name, 
    instructions: agent2Instructions);
#endregion

#region Create a persistent workflow with the two agents



// Create workflow in Foundry
// Build the workflow by adding executors and connecting them
var workflow = new WorkflowBuilder(agent1)
    .AddEdge(agent1, agent2)
    .AddEdge(agent2, agent1)
    .Build();
// Execute the workflow
// await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, "Hello World!"));
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

Console.WriteLine("Agents created successfully.");