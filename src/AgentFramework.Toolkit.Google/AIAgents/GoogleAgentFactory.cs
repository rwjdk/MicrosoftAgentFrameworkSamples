using AgentFramework.Toolkit.AIAgents.Models;
using AgentFramework.Toolkit.AIModels;
using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

// ReSharper disable once CheckNamespace
namespace AgentFramework.Toolkit.AIAgents;

public class GoogleAgentFactory
{
    private readonly string? _apiKey;
    private readonly GoogleAgentFactoryConfiguration? _configuration;

    public GoogleAgentFactory(string apiKey)
    {
        _apiKey = apiKey;
    }

    public GoogleAgentFactory(GoogleAgentFactoryConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Agent GetAgent(GoogleModel model)
    {
        //todo - Middleware
        //todo - Raw Calling interception
        //todo - Timeout???
        //todo - reasoning
        IChatClient client;
        if (_configuration?.Adapter != null)
        {
            client = new GenerativeAIChatClient(_configuration.Adapter, model.ModelName);
        }
        else if (_apiKey != null)
        {
            client = new GenerativeAIChatClient(_apiKey, model.ModelName);
        }
        else
        {
            throw new Exception("Missing Configuration"); //todo - custom exception + better message
        }

        return new Agent(new ChatClientAgent(client));
    }
}