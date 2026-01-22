using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using Shared;
using Shared.Extensions;

namespace Playground.Tests;

#pragma warning disable OPENAI001
public class SpaceNewsWebSearch
{
    public static async Task Run(Secrets secrets)
    {
        OpenAIClient client = new(secrets.OpenAiApiKey);
        //NB: Azure OpenAI is NOT SUPPORTED
        AIAgent agent = client
            .GetResponsesClient("gpt-4.1")
            .AsAIAgent(
                instructions: "You are a Space News AI Reporter",
                tools: [new HostedWebSearchTool()]
            );

        List<AgentResponseUpdate> updates = [];
        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("What is today's news in Space Exploration (List today's date and List only top item)"))
        {
            updates.Add(update);
            Console.Write(update);
        }

        AgentResponse fullResponse = updates.ToAgentResponse();
        fullResponse.Usage.OutputAsInformation();
    }
}