#pragma warning disable OPENAI001
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI;
using Shared.Extensions;

Configuration configuration = ConfigurationManager.GetConfiguration();

//OpenAIClient client = new(configuration.OpenAiApiKey);
AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

AIAgent agent = client
    .GetOpenAIResponseClient("gpt-5-codex")
    .CreateAIAgent(
        instructions: "You are a C# Developer"
    );

List<AgentRunResponseUpdate> updates = [];
await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("Show me an C# Example of a method adding two numbers"))
{
    updates.Add(update);
    Console.Write(update);
}

AgentRunResponse fullResponse = updates.ToAgentRunResponse();
fullResponse.Usage.OutputAsInformation();