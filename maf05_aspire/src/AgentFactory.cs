using Azure.AI.Agents.Persistent;
using Microsoft.Agents.AI;

public static class AgentFactory
{
    // Helper function to read the agent's instructions based on its name
    public static async Task<string> ReadAgentInstructionsAsync(string agent_name)
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
    public static async Task<AIAgent> GenericCreateAgentAsync(string agent_type, string agent_name,
        string? openAIDeploymentName = null, string? openAIApiKeyCredential = null, string? openAIEndpoint = null,
        string? aiFoundryProjectEndpoint = null, string? aiFoundryDeploymentName = null, string? bingConnectionId = null)
    {
        AIAgent _agent = null;

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
            AIAgent openAIMafAgent = new Microsoft.Agents.AI.ChatClientAgent(
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
            var aiFoundryAgentMetadata = await aiFoundryAgentsClient.Administration.CreateAgentAsync(
                model: aiFoundryDeploymentName,
                name: agent_name,
                instructions: await ReadAgentInstructionsAsync(agent_name),
                tools: tools);

            // Extract the created agent from its metadata, but it's still NOT a MAF agent yet.
            var aiFoundryAgent = aiFoundryAgentMetadata.Value;

            // Convert the persistent AI Foundry agent to a MAF agent.
            AIAgent aiFoundryMafAgent = await aiFoundryAgentsClient.GetAIAgentAsync(aiFoundryAgent.Id);

            _agent = aiFoundryMafAgent;
        }

        return _agent;
    }
}

