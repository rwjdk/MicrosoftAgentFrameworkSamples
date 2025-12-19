//YouTube video that cover this sample: https://youtu.be/pqLWICXRtyA

#pragma warning disable OPENAI001
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI;
using OpenAI.Responses;
using Shared.Extensions;

Secrets secrets = SecretManager.GetSecrets();
Console.Clear();
//OpenAIClient client = new(secrets.OpenAiApiKey);
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

AIAgent agent = client
    .GetResponsesClient("gpt-5-codex")
    .CreateAIAgent(
        instructions: "You are a C# Developer"
    );

List<AgentRunResponseUpdate> updates = [];
string question = "Show me an C# Example of a method adding two numbers";
await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(question))
{
    updates.Add(update);
    Console.Write(update);
}

AgentRunResponse fullResponse = updates.ToAgentRunResponse();
fullResponse.Usage.OutputAsInformation();