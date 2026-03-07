# MAF **Azure** OpenAI Client

This **.NET console application** demonstrates how to use:
- the **Native Azure OpenAI SDK** to create an *Azure OpenAI Chat Client*
- the `AsAIAgent` object to create a **MAF Agent** (`AIAgent` class)
- the MAF standard method `RunAsync()` to invoke the agent
- lambda expressions to create in-line functions
- in-line functions as **MAF Tools** to enrich the MAF Agent

---

## 0. Environment variables preparation
Make sure that the following environment variables are defined, using PowerShell or CMD with Administator privileges:
### TL;DR
```bash
setx GITHUB_MODELS_PAT_CLASSIC "***" # afer this, please restart the terminal
$env:GITHUB_MODELS_PAT_CLASSIC = "***" # to make it immediately available in the current terminal session
echo $env:GITHUB_MODELS_PAT_CLASSIC # justo to check, after restarting the terminal
```

### Environment variables setting and getting details:
RETRIEVE variables in PowerShell:
- Current session: <Get-ChildItem Env:> or <Get-ChildItem Env:VARIABLE_NAME>
- Permanent user variables: <Get-ItemProperty HKCU:\Environment> or <Get-ItemProperty 'HKCU:\Environment' -Name VARIABLE_NAME>
- Permanent system variables: <Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment'> or <Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment' -Name VARIABLE_NAME>

SET variables in PowerShell:
- Current session: <$env:VARIABLE_NAME = "value">
- Permanent user variables: <setx VARIABLE_NAME "value">
- Permanent system variables (requires admin): <[Environment]::SetEnvironmentVariable("VARIABLE_NAME", "value", "Machine")>

DELETE variables in PowerShell:
- Current session: <Remove-Item Env:VARIABLE_NAME>
- Permanent user variables: <Remove-ItemProperty -Path HKCU:\Environment -Name VARIABLE_NAME>
- Permanent system variables (requires admin): <[Environment]::SetEnvironmentVariable("VARIABLE_NAME", $null, "Machine")>



## 1. Create the Project

Start by creating a new .NET 9 console application:

```bash
dotnet new console -n maf01_chatcompletion_adapter -f net10.0
```

## 2. Add Required Packages

Install the following NuGet packages::

```bash
dotnet add package OpenAI
dotnet add package Microsoft.Extensions.AI.OpenAI
dotnet add package Microsoft.Agents.AI --prerelease
```
### Why these packages?

- ***Azure.AI.OpenAI*****<br/>
✅ Native Azure OpenAI SDK — creates AzureOpenAI / ChatCompletions / Embeddings clients directly, without MAF or adapters.

- ***Azure.Identity***<br/>
✅ Identity provider library — supplies TokenCredential types (DefaultAzureCredential, AzureCliCredential, etc.) for all Azure SDKs, including Azure OpenAI and Foundry; not related to MAF.

- ***Microsoft.Agents.AI.OpenAI***<br/>
✅ MAF (ME‑AI) adapter package for OpenAI/Azure OpenAI — wraps an existing OpenAI/AzureOpenAI client into an MAF‑compatible provider (AsIChatClient()), or lets you build a full AIAgent using the MAF runtime.

## 3. Open in VS Code
```bash
code .
```


## Documentation


## Coming soon