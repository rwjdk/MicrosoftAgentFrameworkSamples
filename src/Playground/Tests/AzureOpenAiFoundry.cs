using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shared;
using Shared.Extensions;

namespace Playground.Tests;

public class AzureOpenAiFoundry
{
    public static async Task Run(Secrets secrets)
    {
        string sourceName = Guid.NewGuid().ToString("N");
        var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
            .AddSource(sourceName)
            .AddConsoleExporter();
        if (!string.IsNullOrWhiteSpace(secrets.ApplicationInsightsConnectionString))
        {
            tracerProviderBuilder.AddAzureMonitorTraceExporter(options => options.ConnectionString = secrets.ApplicationInsightsConnectionString);
        }

        using var tracerProvider = tracerProviderBuilder.Build();

        PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

        BingGroundingSearchConfiguration bingToolConfiguration = new(secrets.BingApiKey);
        BingGroundingSearchToolParameters bingToolParameters = new([bingToolConfiguration]);

        Response<PersistentAgent>? aiFoundryAgent = null;
        ChatClientAgentSession? chatClientAgentThread = null;
        try
        {
            aiFoundryAgent = await client.Administration.CreateAgentAsync(
                secrets.ChatDeploymentName,
                "PlaygroundAgent",
                "Some description",
                "You are a nice AI",
                new List<ToolDefinition>
                {
                    new BingGroundingToolDefinition(bingToolParameters)
                });

            AIAgent agent = (await client.GetAIAgentAsync(aiFoundryAgent.Value.Id))
                .AsBuilder()
                .UseOpenTelemetry(sourceName: sourceName, telemetryAgent => { telemetryAgent.EnableSensitiveData = true; })
                .Build();

            AgentSession session = await agent.CreateSessionAsync();

            List<AgentResponseUpdate> updates = [];
            await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("What is today's news in Space Exploration (List today's date and List only top item)", session))
            {
                updates.Add(update);
                Console.Write(update);
            }

            AgentResponse fullResponse = updates.ToAgentResponse();
            fullResponse.Usage.OutputAsInformation();

            //Get citations
            foreach (ChatMessage message in fullResponse.Messages)
            {
                foreach (AIContent content in message.Contents)
                {
                    foreach (AIAnnotation annotation in content.Annotations ?? [])
                    {
                        if (annotation is CitationAnnotation citationAnnotation)
                        {
                            Utils.Yellow("Source: " + citationAnnotation.Title + " (" + citationAnnotation.Url + ")");
                        }
                    }
                }
            }
        }
        finally
        {
            if (chatClientAgentThread != null)
            {
                await client.Threads.DeleteThreadAsync(chatClientAgentThread.ConversationId);
            }

            if (aiFoundryAgent != null)
            {
                await client.Administration.DeleteAgentAsync(aiFoundryAgent.Value.Id);
            }
        }
    }
}