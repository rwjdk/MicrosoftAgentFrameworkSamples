using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using Shared.Extensions;

namespace Playground.Tests;

#pragma warning disable OPENAI001
public class SpaceNewsWebSearch
{
    public static async Task Run(Configuration configuration)
    {
        OpenAIClient client = new(configuration.OpenAiApiKey);
        //NB: Azure OpenAI is NOT SUPPORTED
        AIAgent agent = client
            .GetOpenAIResponseClient("gpt-4.1")
            .CreateAIAgent(
                instructions: "You are a Space News AI Reporter",
                tools: [new HostedWebSearchTool()]
            );

        List<AgentRunResponseUpdate> updates = [];
        await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("What is today's news in Space Exploration (List today's date and List only top item)"))
        {
            updates.Add(update);
            Console.Write(update);
        }

        AgentRunResponse fullResponse = updates.ToAgentRunResponse();
        fullResponse.Usage.OutputAsInformation();
    }
}