
# AI Foundry V2 Workflow Prototype with MAF

This sample demonstrates how to build a **.NET console application** that integrates **AI Foundry V2 agents** with the **Microsoft Agent Framework (MAFThis sample demonstrates how to build a **.NET console application** that integrates **AI Foundry V2 agents** with the **Microsoft Agent Framework (MAF)** for orchestration and tool interoperability.

---

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

// 1) Configure Foundry client
var credential = new DefaultAzureCredential();
var foundryClient = new FoundryClient(new Uri("https://<your-foundry-endpoint>.azure.com"), credential);

// 2) Create two persistent agents
var agent1 = await foundryClient.CreateAgentAsync(new CreateAgentOptions
{
    Name = "WriterAgent",
    Instructions = "Write creative and engaging stories."
});

var agent2 = await foundryClient.CreateAgentAsync(new CreateAgentOptions
{
    Name = "ReviewerAgent",
    Instructions = "Review text for clarity and tone."
});

// 3) Wrap agents with MAF abstractions (optional)
var mafAgent1 = new AzureAIAgent(agent1);
var mafAgent
```

## 5. Next steps<br/>
- Use Workflow Designer in AI Foundry V2 to orchestrate these agents visually or programmatically.
- Add MAF workflows for advanced orchestration and tool integration.
- Explore memory store and multi-turn threads for stateful conversations.