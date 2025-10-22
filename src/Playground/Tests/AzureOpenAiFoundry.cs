using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;

namespace Playground.Tests;

public class AzureOpenAiFoundry
{
    public static async Task Run(Configuration configuration)
    {
        PersistentAgentsClient client = new(configuration.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

        BingGroundingSearchConfiguration bingToolConfiguration = new(configuration.BingApiKey);
        BingGroundingSearchToolParameters bingToolParameters = new([bingToolConfiguration]);

        Response<PersistentAgent>? aiFoundryAgent = null;
        try
        {
            aiFoundryAgent = await client.Administration.CreateAgentAsync(
                configuration.ChatDeploymentName,
                "MyFirstAgent",
                "Some description",
                "You are a nice AI",
                new List<ToolDefinition>
                {
                    new BingGroundingToolDefinition(bingToolParameters)
                });

            ChatClientAgent agent = await client.GetAIAgentAsync(aiFoundryAgent.Value.Id);

            AgentThread thread = agent.GetNewThread();

            List<AgentRunResponseUpdate> updates = [];
            await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("What is today's news in Space Exploration (List today's date and List only top item)", thread))
            {
                updates.Add(update);
                Console.Write(update);
            }

            AgentRunResponse fullResponse = updates.ToAgentRunResponse();
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
                            Utils.WriteLineYellow("Source: " + citationAnnotation.Title + " (" + citationAnnotation.Url + ")");
                        }
                    }
                }
            }
        }
        finally
        {
            if (aiFoundryAgent != null)
            {
                await client.Administration.DeleteAgentAsync(aiFoundryAgent.Value.Id);
            }
        }
    }
}