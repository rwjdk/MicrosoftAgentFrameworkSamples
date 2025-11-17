using AgentFramework.Toolkit.Agents;
using AgentFramework.Toolkit.Agents.Models;
using AgentFramework.Toolkit.AnthropicSDK.Agents.Models;
using Anthropic.SDK;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFramework.Toolkit.AnthropicSDK.Agents;

public class AgentFactoryAnthropicSDK(AnthropicSDKConnection connection)
{
    public Agent CreateAgent(AnthropicSDKOptions options)
    {
        IChatClient client = GetClient(options);

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options, options));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.AnthropicSDK);
        }

        return new Agent(innerAgent, Provider.AnthropicSDK);
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(AgentOptions options, AnthropicSDKOptions? anthropicSDKOptions)
    {
        ChatOptions chatOptions = new()
        {
            ModelId = options.DeploymentModelName
        };

        if (options.Tools != null)
        {
            chatOptions.Tools = options.Tools;
        }

        if (options.MaxOutputTokens.HasValue)
        {
            chatOptions.MaxOutputTokens = options.MaxOutputTokens.Value;
        }

        if (anthropicSDKOptions?.Temperature != null)
        {
            chatOptions.Temperature = anthropicSDKOptions.Temperature;
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            
            Name = options.Name,
            Instructions = options.Instructions,
            Description = options.Description,
            Id = options.Id
        };
        
        chatClientAgentOptions.ChatOptions = chatOptions;

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }


    private IChatClient GetClient(AgentOptions options)
    {
        HttpClient? httpClient = null;

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            httpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
        }
        
        if (options.NetworkTimeout.HasValue)
        {
            httpClient ??= new HttpClient();
            httpClient.Timeout = options.NetworkTimeout.Value;
        }

        AnthropicClient anthropicClient = new(new APIAuthentication(connection.ApiKey), httpClient);
        IChatClient client = anthropicClient.Messages.AsBuilder().Build();
        return client;
    }
}