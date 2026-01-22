//YouTube video that cover this sample: https://youtu.be/tTChhtkFk3M

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();
PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

BingGroundingSearchConfiguration bingToolConfiguration = new(secrets.BingApiKey);
BingGroundingSearchToolParameters bingToolParameters = new([bingToolConfiguration]);

Response<PersistentAgent>? aiFoundryAgent = null;
ChatClientAgentThread? chatClientAgentThread = null;
try
{
    aiFoundryAgent = await client.Administration.CreateAgentAsync(
        secrets.ChatDeploymentName,
        "CurrentNewsAgent",
        "",
        "You report about Space News",
        tools: new List<ToolDefinition>
        {
            new BingGroundingToolDefinition(bingToolParameters)
        });

    AIAgent agent = (await client.GetAIAgentAsync(aiFoundryAgent.Value.Id));

    AgentThread thread = await agent.GetNewThreadAsync();

    List<AgentResponseUpdate> updates = [];
    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("What is today's news in Space Exploration (List today's date and List only top item)", thread))
    {
        updates.Add(update);
        Console.Write(update);
    }

    AgentResponse fullResponse = updates.ToAgentResponse();
    fullResponse.Usage.OutputAsInformation();

    //NB: Do not support citations like the Responses API
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