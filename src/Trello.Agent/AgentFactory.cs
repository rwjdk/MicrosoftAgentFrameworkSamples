using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;
using System.Reflection;
using MicrosoftAgentFramework.Toolkit.AITools;
using Trello.Agent.Models;
using Trello.Agent.Tools;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Options.AddCardOptions;
using TrelloDotNet.Model.Options.GetBoardOptions;
using TrelloDotNet.Model.Options.GetCardOptions;
using TrelloDotNet.Model.Options.GetListOptions;

namespace Trello.Agent;

public class AgentFactory(AzureOpenAIClient azureOpenAiClient, TrelloCredentials trelloCredentials)
{
    public TrelloClient TrelloClient { get; } = new TrelloClient(trelloCredentials.ApiKey, trelloCredentials.Token);

    public ChatClientAgent GetOrchestratorAgent()
    {
        return azureOpenAiClient
            .GetChatClient("gpt-4.1")
            .CreateAIAgent(
                name: "Orchestrator",
                instructions: "You are an Orchestrator Agent in charge of completing the users Trello-specific Tasks. You should always hand over the task to one of your fellow Agents");
    }

    public async Task<ChatClientAgent> GetTrelloAgent()
    {
        TrelloInformationTools trelloInformationTools = new(TrelloClient);
        var aiTools = AIToolsFactory.GetToolsFromMethods(trelloInformationTools);

        IList<AITool> tools = AIToolsFactory.GetToolsFromAttribute(new SimpleTools("x"));


        return azureOpenAiClient
            .GetChatClient("gpt-5-mini")
            .CreateAIAgent(
                name: "TrelloInformation",
                instructions: """
                              You are Trello Information Agent that have various tools to 
                              give the user information about their Trello data",
                              Never ask question. Use your best judgement
                              """,
                tools: aiTools);
    }
}