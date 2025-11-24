
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
dotnet add package Microsoft.Agents.AI.AzureAI --dotnet add package Microsoft.Agents.AI.AzureAI --prerelease
```
## Why these packages?
- ***Azure.AI.Agents.Persistent***
  The official SDK for **AI Foundry V2**. It supports:
  - Persistent agents
  - Multi-turn conversations
  - Thread and memory store management

- ***Microsoft.Agents.AI.AzureAI***
  Bridges AI Foundry V2 with MAF, providing:
  - A consistent API for orchestration
  - Integration with MAF abstractions (AIAgent, workflows, tools)

- ***Azure.Identity***
  Handles authentication using Azure credentials (e.g., DefaultAzureCredential).

## 3. Open in VS Code
```bash
code .
```