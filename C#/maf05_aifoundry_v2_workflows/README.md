# AI Foundry V2 Workflow Prototype with MAF

This sample demonstrates how to build a **.NET console application** that integrates **AI Foundry V2 agents** with the **Microsoft Agent Framework (MAF)** for orchestration and tool interoperability.


## Sequential Orchestrator in AI Foundry Workflows

A **Sequential Orchestrator** executes actions in a strict linear order, where each step depends on the completion of the previous one. This pattern is ideal for multi-stage processes such as **draft → review → translate**, ensuring that intermediate outputs flow into subsequent steps.

### Why Sequential?
- Each agent adds value that the next agent consumes.
- Execution is deterministic and easy to visualize.
- Perfect for pipelines where steps cannot run in parallel.

### Example: Writer → Translator Workflow
Below is a YAML definition for a simple sequential workflow in **AI Foundry V2**:

```yaml
kind: workflow
name: writer-and-translator-workflow
description: ""
id: ""

trigger:
  kind: OnConversationStart
  id: trigger_wf
  actions:
    - kind: InvokeAzureAgent
      id: writer
      agent:
        name: WriterAgent
      input:
        messages: =System.LastMessage
      output:
        messages: Local.writerOutput
        autoSend: false

    - kind: InvokeAzureAgent
      id: translator
      agent:
        name: TranslatorAgent
      input:
        messages: =Local.writerOutput
      output:
        autoSend: true

    - kind: End    - kind: EndConversation
```

---

## 0. Environment variables preparation
Make sure that the following environment variables are defined, using PowerShell or CMD with Administator privileges:
```bash
setx AIF_BASPROJECT_ENDPOINT "https://<aifoundryService>.services.ai.azure.com/api/projects/<projectName>" # afer this, please restart the terminal
$env:AZURE_OPENAI_CHAT_DEPLOYMENT_NAME = "gpt-4o-mini" # to make it immediately available in the current terminal session
echo $env:AIF_BASPROJECT_ENDPOINT # justo to check, after restarting the terminal
```

## 1. Create the Project

Start by creating a new .NET 9 console application:

```bash
dotnet new console -n maf05_aifoundry_v2_workflows -f net9.0
```

## 2. Add Required Packages

Install the following NuGet packages::

```bash
dotnet add package Azure.AI.Agents.Persistent --prerelease
dotnet add package Microsoft.Agents.AI.AzureAI --prerelease
dotnet add package Azure.Identity
```
### Why these packages?
- ***Azure.AI.Agents.Persistent***<br/>
  The official SDK for **AI Foundry V2**. It supports:
  - Persistent agents
  - Multi-turn conversations
  - Thread and memory store management

- ***Microsoft.Agents.AI.AzureAI***<br/>
  Bridges AI Foundry V2 with MAF, providing:
  - A consistent API for orchestration
  - Integration with MAF abstractions (AIAgent, workflows, tools)

- ***Azure.Identity***<br/>
  Handles authentication using Azure credentials (e.g., DefaultAzureCredential).

## 3. Open in VS Code
```bash
code .
```
## 4. Initialize your agents<br/>
Here's an example of creating two AI Foundry V2 agents in C#:
```bash

using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI.AzureAI;

// 1) Create a Foundry client to create/retrieve server side agents.
var aiFoundryProjectClient = new AIProjectClient(
    new Uri(projectEndpoint), 
    new Azure.Identity.AzureCliCredential());

// 2) Create two persistent agents
AIAgent agent1 = await aiFoundryProjectClient.CreateAgentAsync(
    model: deploymentName, 
    name: agent1Name, 
    instructions: agent1Instructions);

AIAgent agent2 = await aiFoundryProjectClient.CreateAIAgentAsync(
    model: deploymentName, 
    name: agent2Name, 
    instructions: agent2Instructions);

// 3) Wrap agents with MAF abstractions (optional)
var mafAgent1 = new AzureAIAgent(agent1);
var mafAgent
```

## 5. Next steps<br/>
- Use Workflow Designer in AI Foundry V2 to orchestrate these agents visually or programmatically.
- Add MAF workflows for advanced orchestration and tool integration.
- Explore memory store and multi-turn threads for stateful conversations.

## Documentation
Inspired by [Agent with AzureAIProject sample](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/AgentProviders/Agent_With_AzureAIProject)


## Coming soon
- Workflow Designer integration guide
- Python equivalent examples for V1 vs V2
- Architecture diagram for multi-agent orchestration