//YouTube video that cover this sample: https://youtu.be/pqLWICXRtyA

#pragma warning disable OPENAI001
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI;
using Shared.Extensions;

Configuration configuration = ConfigurationManager.GetConfiguration();
Console.Clear();
//OpenAIClient client = new(configuration.OpenAiApiKey);
AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

AIAgent agent = client
    .GetOpenAIResponseClient("gpt-5-codex")
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