﻿// inspired by https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/GettingStarted/AgentProviders/Agent_With_AzureAIProject/Program.cs

// make sure that the following environment variables are defined, using PowerShell or CMD with Administator privileges:
// setx AIF_BASPROJECT_ENDPOINT "https://aif1bassvj36b.services.ai.azure.com/api/projects/aif1basswcprj01" # afer this, please restart the terminal
// $env:AZURE_OPENAI_CHAT_DEPLOYMENT_NAME = "gpt-4o" # to make it immediate in the current terminal session
// check with echo $env:AIF_BASPROJECT_ENDPOINT (after restarting the terminal) that the variables are set correctly.

# region Constants and Variables
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Azure.AI.Projects;

var projectEndpoint = Environment.GetEnvironmentVariable("AIF_BASPROJECT_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

const string agent1Name = "WriterAgent";
const string agent2Name = "TranslatorAgent";

const string agent1Instructions = "You are good at telling jokes. You speak English only, even if the question is in another language.";
const string agent2Instructions = "You are good at translating text. You understand all languages, but speak Italian only.";
#endregion

// Get a client to create/retrieve server side agents with.
var aiFoundryProjectClient = new AIProjectClient(
    new Uri(projectEndpoint!), 
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

Console.WriteLine("Agents created successfully.");