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
ChatClientAgentSession? chatClientAgentSession = null;
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

    AgentSession session = await agent.CreateSessionAsync();

    List<AgentResponseUpdate> updates = [];
    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("What is today's news in Space Exploration (List today's date and List only top item)", session))
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
    if (chatClientAgentSession != null)
    {
        await client.Threads.DeleteThreadAsync(chatClientAgentSession.ConversationId);
    }

    if (aiFoundryAgent != null)
    {
        await client.Administration.DeleteAgentAsync(aiFoundryAgent.Value.Id);
    }
}