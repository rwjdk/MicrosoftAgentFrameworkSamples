using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFramework.Utilities.GoogleGenerativeAI;

public class GoogleGenerativeAIAgentFactory(GoogleGenerativeAIConnection connection)
{
    private readonly GoogleGenerativeAIConnection? _connection = connection;

    public Agent CreateAgent(GoogleGenerativeAIOptions options)
    {
        IChatClient client = GetClient(options.DeploymentModelName);

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), AgentProvider.GoogleGenerativeAI);
        }

        return new Agent(innerAgent, AgentProvider.GoogleGenerativeAI);
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(GoogleGenerativeAIOptions options)
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

        if (options.ThinkingBudget > 0)
        {
            anyOptionsSet = true;
            chatOptions.AdditionalProperties = new AdditionalPropertiesDictionary
            {
                ["ThinkingBudget"] = options.ThinkingBudget,
            };
        }


        if (options.Temperature != null)
        {
            anyOptionsSet = true;
            chatOptions.Temperature = options.Temperature;
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