using AgentFramework.Toolkit.Agents;
using AgentFramework.Toolkit.Agents.Models;
using AgentFramework.Toolkit.GoogleGenerativeAI;
using AgentFramework.Toolkit.GoogleGenerativeAI.Agents.Models;
using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

// ReSharper disable once CheckNamespace
namespace AgentFramework.Toolkit.AIAgents;

public class AgentFactoryGoogleGenerativeAI(GoogleGenerativeAIConnection connection)
{
    private readonly GoogleGenerativeAIConnection? _connection = connection;

    public Agent CreateAgent(GoogleGenerativeAIOptions options)
    {
        IChatClient client = GetClient(options.DeploymentModelName);

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options, options));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.GoogleGenerativeAI);
        }

        return new Agent(innerAgent, Provider.GoogleGenerativeAI);
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(AgentOptions options, GoogleGenerativeAIOptions? googleGenerativeAIOptions)
    {
        bool anyOptionsSet = false;
        ChatOptions chatOptions = new();
        if (options.Tools != null)
        {
            anyOptionsSet = true;
            chatOptions.Tools = options.Tools;
        }

        if (options.MaxOutputTokens.HasValue)
        {
            anyOptionsSet = true;
            chatOptions.MaxOutputTokens = options.MaxOutputTokens.Value;
        }

        if (googleGenerativeAIOptions?.Temperature != null)
        {
            anyOptionsSet = true;
            chatOptions.Temperature = googleGenerativeAIOptions.Temperature;
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Instructions = options.Instructions,
            Description = options.Description,
            Id = options.Id
        };
        if (anyOptionsSet)
        {
            chatClientAgentOptions.ChatOptions = chatOptions;
        }

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }


    private IChatClient GetClient(string model)
    {
        IChatClient client;
        if (_connection?.Adapter != null)
        {
            client = new GenerativeAIChatClient(_connection.Adapter, model);
        }
        else if (_connection?.ApiKey != null)
        {
            client = new GenerativeAIChatClient(_connection.ApiKey, model);
        }
        else
        {
            throw new Exception("Missing Configuration"); //todo - custom exception + better message
        }

        //todo - Timeout???
        //Todo - Can RawHttpCallDetails somehow be supported?

        return client;
    }
}