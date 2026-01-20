// See https://aka.ms/new-console-template for more information

/*
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
*/


// read variables from environment with <$env:VARIABLE_NAME>
// create with <setx VARIABLE_NAME value>
// delete with <Remove-ItemProperty -Path HKCU:\Environment -Name MyVariableName>


using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

var question = "Write a short story about a haunted house.";
var cc = new OpenAI.Chat.ChatClient("gpt-4o-mini",
    new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_MODELS_PAT_CLASSIC")!),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") });


// Send a chat request using the vendor-specific client
OpenAI.Chat.ChatCompletion response_openAI = (await cc.CompleteChatAsync(question)).Value;

// Read the model's reply
Console.WriteLine(response_openAI.Content[0].Text);

Console.WriteLine("\n\n\n\n*************\n\n\n\n\n");

IChatClient cc_adapter = cc.AsIChatClient(); // extension method to adapt to Agent Framework IChatClient

AIAgent writer = new ChatClientAgent( // Agent Framework agent wrapping the chat client
    chatClient: cc_adapter,
    name: "Writer",
    instructions: "You are a helpful writing assistant."
);


AgentRunResponse response = await writer.RunAsync(question);
Console.WriteLine(response.Text);

// This time we print the story with streaming
await foreach(var chunk in writer.RunStreamingAsync(question))
{
    Console.Write(chunk.Text);
}


Console.WriteLine("\nThe program has ended!");