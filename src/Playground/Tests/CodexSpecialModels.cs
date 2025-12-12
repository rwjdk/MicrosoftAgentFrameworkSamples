using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using Shared;
using Shared.Extensions;

#pragma warning disable OPENAI001

namespace Playground.Tests;

public class CodexSpecialModels
{
    public static async Task Run(Secrets secrets)
    {
        //OpenAIClient client = new(secrets.OpenAiApiKey);
        AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
        AIAgent agent = client.GetOpenAIResponseClient("gpt-5-codex")
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
    }
}