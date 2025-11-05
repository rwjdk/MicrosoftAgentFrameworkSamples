// Copyright (c) Microsoft. All rights reserved.

using System.ClientModel;
using A2A;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using Shared;

namespace Agent2Agent.Client;

internal sealed class HostClientAgent
{
    internal HostClientAgent(ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger("HostClientAgent");
    }

    internal async Task InitializeAgentAsync(string[] agentUrls)
    {
        Configuration configuration = Shared.ConfigurationManager.GetConfiguration();

        try
        {
            // Connect to the remote agents via A2A
            var createAgentTasks = agentUrls.Select(CreateAgentAsync);
            var agents = await Task.WhenAll(createAgentTasks);
            var tools = agents.Select(agent => (AITool)agent.AsAIFunction()).ToList();

            // Create the agent that uses the remote agents as tools
            this.Agent = new AzureOpenAIClient(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey))
                .GetChatClient("gpt-4.1")
                .CreateAIAgent(instructions: "You specialize in handling queries for users and using your tools to provide answers.", name: "HostClient", tools: tools);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to initialize HostClientAgent");
            throw;
        }
    }

    /// <summary>
    /// The associated <see cref="Agent"/>
    /// </summary>
    public AIAgent? Agent { get; private set; }

    #region private

    private readonly ILogger _logger;

    private static async Task<AIAgent> CreateAgentAsync(string agentUri)
    {
        var url = new Uri(agentUri);
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        var agentCardResolver = new A2ACardResolver(url, httpClient);

        try
        {
            return await agentCardResolver.GetAIAgentAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    #endregion
}