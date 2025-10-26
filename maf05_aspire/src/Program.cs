using Microsoft.Agents.AI;
using static AgentFactory;

record ChatInput(string input); // record declaration: it's a compact form to define an immutable type with public properties.

public class Program
{
    public static async Task Main(string[] args)
    {
        #region Setup environment variables
        var aiFoundryProjectEndpoint = Environment.GetEnvironmentVariable("AIF_STD_PROJECT_ENDPOINT"!);
        var openAIEndpoint = "https://models.github.ai/inference";
        var openAIDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
        var aiFoundryDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
        var aiFoundryBingConnectionId = Environment.GetEnvironmentVariable("BING_CONNECTION_ID") ?? null;
        var openAIApiKeyCredential = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!;
        #endregion

        #region OpenAI MAF Agent: CreatureQuestioner
        var openAIMafAgentCreatureQuestioner = await GenericCreateAgentAsync(
            agent_type: "openaiMafAgent",
            agent_name: "CreatureQuestioner",
            openAIDeploymentName: openAIDeploymentName,
            openAIEndpoint: openAIEndpoint,
            openAIApiKeyCredential: openAIApiKeyCredential);

        // Test the OpenAI MAF Agent
        /*Microsoft.Agents.AI.AgentRunResponse openAIMafAgentCreatureQuestionerResponse = await openAIMafAgentCreatureQuestioner.RunAsync("never mind");
        Console.WriteLine(openAIMafAgentCreatureQuestionerResponse.Text);*/
        #endregion

        #region AI Foundry MAF Agent with Bing Grounding Tool: AnimalPicker
        var aiFoundryMafAgentAnimalPicker = await GenericCreateAgentAsync(
            agent_type: "aiFoundryMafAgent",
            agent_name: "AnimalPicker",
            aiFoundryProjectEndpoint: aiFoundryProjectEndpoint,
            aiFoundryDeploymentName: aiFoundryDeploymentName,
            bingConnectionId: aiFoundryBingConnectionId);

        // Test the AI Foundry MAF Agent
        /*Microsoft.Agents.AI.AgentThread thread = aiFoundryMafAgentAnimalPicker.GetNewThread();
        Console.WriteLine(await aiFoundryMafAgentAnimalPicker.RunAsync(openAIMafAgentCreatureQuestionerResponse.Text, thread));*/
        #endregion

        #region builder and DI registry (simple agent dictionary)
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddKeyedSingleton<AIAgent>("openai", (sp, key) => openAIMafAgentCreatureQuestioner)
            .AddKeyedSingleton<AIAgent>("foundry", (sp, key) => aiFoundryMafAgentAnimalPicker);
        #endregion

        #region app build and mappings
        Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();

        app.MapGet("/", () => Results.Ok(new
        {
            hello = "Use POST /chat/{foundry|openai} with { \"input\": \"...\" }"
        }));

        // POST /chat/{agent}   agent = foundry | openai
        app.MapPost("/chat/{agent}", async (string agent, ChatInput body) =>
        {
            var agent_name = agent.ToLowerInvariant();

            if (agent_name != "openai" && agent_name != "foundry")
            {
                return Results.BadRequest(new { error = $"Agent must be 'foundry' or 'openai', not '{agent}'" });
            }

            var selected = app.Services.GetRequiredKeyedService<AIAgent>(agent_name);

            AgentRunResponse resp = await selected.RunAsync(body.input);
            return Results.Ok(new { agent, text = resp.Text });
        });

        app.Run();
        #endregion
    }
}