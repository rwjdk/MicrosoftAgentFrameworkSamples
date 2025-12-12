//YouTube video that cover this sample: https://youtu.be/g72ks3rY9qQ

using A2A;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI;

Utils.WriteLineDarkGray("Initializing");
Utils.WriteLineDarkGray("- Waiting 1 sec for the server to be ready");
await Task.Delay(1000);
Secrets secrets = SecretManager.GetConfiguration();

Utils.WriteLineDarkGray("- Connecting to Remote Agent");
A2ACardResolver agentCardResolver = new A2ACardResolver(new Uri("http://localhost:5000/"));
AIAgent remoteAgent = await agentCardResolver.GetAIAgentAsync();

Utils.Separator();

Utils.WriteLineDarkGray("Ready for questions");
AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
ChatClientAgent agent = azureOpenAIClient
    .GetChatClient("gpt-4.1")
    .CreateAIAgent(
        name: "ClientAgent",
        instructions: "You specialize in handling queries for users and using your tools to provide answers.",
        tools: [remoteAgent.AsAIFunction()]);

AgentThread thread = agent.GetNewThread();
while (true)
{
    Console.Write("> ");
    string message = Console.ReadLine() ?? string.Empty;
    if (message == string.Empty)
    {
        continue;
    }

    AgentRunResponse response = await agent.RunAsync(message, thread);
    Console.WriteLine(response);
}