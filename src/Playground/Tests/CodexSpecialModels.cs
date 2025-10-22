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
    public static async Task Run(Configuration configuration)
    {
        //OpenAIClient client = new(configuration.OpenAiApiKey);
        AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));
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