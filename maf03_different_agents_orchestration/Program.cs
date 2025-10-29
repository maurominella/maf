using Azure.AI.Agents.Persistent;
using Microsoft.Agents.AI.Workflows;

#region Setup environment variables
var aiFoundryProjectEndpoint = Environment.GetEnvironmentVariable("AIF_STD_PROJECT_ENDPOINT"!);
var openAIEndpoint = "https://models.github.ai/inference";
var openAIDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
var aiFoundryDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
var aiFoundryBingConnectionId = Environment.GetEnvironmentVariable("BING_CONNECTION_ID") ?? null;
var openAIApiKeyCredential = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!;
#endregion

#region CreatureQuestioner - OpenAI MAF Agent
Microsoft.Agents.AI.AIAgent openAIMafAgentCreatureQuestioner = await GenericCreateAgentAsync(
    agent_type: "openaiMafAgent",
    agent_name: "CreatureQuestioner",
    openAIDeploymentName: openAIDeploymentName,
    openAIEndpoint: openAIEndpoint,
    openAIApiKeyCredential: openAIApiKeyCredential);

// Test the OpenAI MAF Agent
Microsoft.Agents.AI.AgentRunResponse openAIMafAgentCreatureQuestionerResponse = await openAIMafAgentCreatureQuestioner.RunAsync("never mind");
Console.WriteLine(openAIMafAgentCreatureQuestionerResponse.Text);
#endregion

#region AnimalPicker - AI Foundry MAF Agent with Bing Grounding Tool
Microsoft.Agents.AI.AIAgent aiFoundryMafAgentAnimalPicker = await GenericCreateAgentAsync(
    agent_type: "aiFoundryMafAgent",
    agent_name: "AnimalPicker",
    aiFoundryProjectEndpoint: aiFoundryProjectEndpoint,
    aiFoundryDeploymentName: aiFoundryDeploymentName,
    bingConnectionId: aiFoundryBingConnectionId);

// Test the AI Foundry MAF Agent
Microsoft.Agents.AI.AgentThread thread = aiFoundryMafAgentAnimalPicker.GetNewThread();
var aiFoundryMafAgentAnimalPickerResponse = await aiFoundryMafAgentAnimalPicker.RunAsync(openAIMafAgentCreatureQuestionerResponse.Text, thread);
Console.WriteLine(aiFoundryMafAgentAnimalPickerResponse.Text);
#endregion

#region Workflow connecting the different agents
// Create a workflow that connects the new different agents
Microsoft.Agents.AI.Workflows.Workflow mafWorkflow =
    Microsoft.Agents.AI.Workflows.AgentWorkflowBuilder
        .BuildSequential(openAIMafAgentCreatureQuestioner, aiFoundryMafAgentAnimalPicker);

// AsAgentAsync is defined in Microsoft.Agents.AI.Workflows...
// ...and returns a MAF AIAgent that represents the entire workflow
Microsoft.Agents.AI.AIAgent mafAgentWorkflow = await mafWorkflow.AsAgentAsync();

await foreach (var responseChunk in mafAgentWorkflow.RunStreamingAsync("Never mind"))
{
    Console.Write(responseChunk.Text);
}
#endregion

#region Cleanup 
// await aiFoundryMafAgentAnimalPicker.ChatClient.Administration.DeleteAgentAsync(aiFoundryMafAgentAnimalPicker.Id);
#endregion

Console.WriteLine("\nDone.");

// Helper function to read the agent's instructions based on its name
static async Task<string> ReadAgentInstructionsAsync(string agent_name)
{
    string filePath = Path.Combine("agents", $"{agent_name}.txt");

    if (!File.Exists(filePath))
    {
        return "You are a clever agent";
    }

    try
    {
        return await File.ReadAllTextAsync(filePath);
    }
    catch (IOException ex)
    {
        return $"IO error occurred: {ex.Message}";
    }
    catch (UnauthorizedAccessException ex)
    {
        return $"Access error occurred: {ex.Message}";
    }
}


// Single Chat function for all kinds of agents
static async Task<Microsoft.Agents.AI.AIAgent> GenericCreateAgentAsync(string agent_type, string agent_name,
    string? openAIDeploymentName = null, string? openAIApiKeyCredential = null, string? openAIEndpoint = null,
    string? aiFoundryProjectEndpoint = null, string? aiFoundryDeploymentName = null, string? bingConnectionId = null)
{
    Microsoft.Agents.AI.AIAgent _agent = null;

    if (agent_type == "openaiMafAgent")
    {
        // Create the vendor-specific ChatClient
        var cc = new OpenAI.Chat.ChatClient(
            openAIDeploymentName,
            new System.ClientModel.ApiKeyCredential(openAIApiKeyCredential!),
            new OpenAI.OpenAIClientOptions { Endpoint = new Uri(openAIEndpoint!) });

        // Using AsIChatClient method from Microsoft.Extensions.AI.OpenAI package...
        // ...to adapt the OpenAI ChatClient to the MAF IChatClient interface defined in Microsoft.Extensions.AI
        Microsoft.Extensions.AI.IChatClient cc_adapter = Microsoft.Extensions.AI.OpenAIClientExtensions.AsIChatClient(cc);

        // Create a ChatClientAgent object, which is a general-purpose agent that can talk to any IChatClient implementation.
        // Convert to the MAF AIAgent type to leverage polymorphism
        Microsoft.Agents.AI.AIAgent openAIMafAgent = new Microsoft.Agents.AI.ChatClientAgent(
            chatClient: cc_adapter, new Microsoft.Agents.AI.ChatClientAgentOptions()
            {
                Name = agent_name,
                Instructions = await ReadAgentInstructionsAsync(agent_name),
            });

        _agent = openAIMafAgent;
    }

    else if (agent_type == "aiFoundryMafAgent")
    {

        var tools = new List<ToolDefinition>(); // create a generic tools variable

        // Get a client to create/retrieve server side agents with.
        var aiFoundryAgentsClient = new Azure.AI.Agents.Persistent.PersistentAgentsClient(
            aiFoundryProjectEndpoint,
            new Azure.Identity.AzureCliCredential());

        if (bingConnectionId is not null)
        {
            BingGroundingToolDefinition bingGroundingTool = new(
                bingGrounding: new BingGroundingSearchToolParameters(
                    [new BingGroundingSearchConfiguration(connectionId: bingConnectionId)])
            );
            tools.Add(bingGroundingTool);
        }

        // The following instruction creates the AI Foundry Agent in the AI Foundry project and returns its metadata.
        // It's still NOT a MAF agent yet.
        Azure.AI.Agents.Persistent.PersistentAgent aiFoundryAgent = (await aiFoundryAgentsClient.Administration.CreateAgentAsync(
            model: aiFoundryDeploymentName,
            name: agent_name,
            instructions: await ReadAgentInstructionsAsync(agent_name),
            tools: tools)).Value;


        // Convert the persistent AI Foundry agent to a MAF agent.
        Microsoft.Agents.AI.AIAgent aiFoundryMafAgent = await aiFoundryAgentsClient.GetAIAgentAsync(aiFoundryAgent.Id);

        _agent = aiFoundryMafAgent;
    }

    return _agent;
}