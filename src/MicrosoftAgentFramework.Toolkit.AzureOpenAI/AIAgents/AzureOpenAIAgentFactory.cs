using System;
using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using MicrosoftAgentFramework.Toolkit.AIModels;
using OpenAI;

// ReSharper disable once CheckNamespace
namespace MicrosoftAgentFramework.Toolkit.AIAgents;

public class AzureOpenAIAgentFactory
{
    private readonly AzureOpenAIAgentFactoryConfiguration _configuration;

    public AzureOpenAIAgentFactory(string endpoint, string apiKey)
    {
        _configuration = new AzureOpenAIAgentFactoryConfiguration
        {
            Endpoint = endpoint,
            ApiKey = apiKey
        };
    }

    public AzureOpenAIAgentFactory(AzureOpenAIAgentFactoryConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Agent GetAgent(AzureOpenAIModel model)
    {
        //todo - support RBAC
        AzureOpenAIClientOptions options;
        if (_configuration.AzureOpenAIClientOptions == null)
        {
            options = new AzureOpenAIClientOptions
            {
                NetworkTimeout = model.NetworkTimeout,
            };
        }
        else
        {
            options = _configuration.AzureOpenAIClientOptions;
            options.NetworkTimeout = model.NetworkTimeout;
        }

        AzureOpenAIClient client = new(
            new Uri(_configuration.Endpoint),
            new ApiKeyCredential(_configuration.ApiKey!),
            options);

        //todo - Middleware
        //todo - Raw Calling interception
        //todo - Timeout
        //todo - reasoning
        switch (model.ClientType)
        {
            case AIModelClientType.ChatClient:
            {
                ChatClientAgent inner = client.GetChatClient(model.ModelName).CreateAIAgent();
                return new Agent(inner);
            }
            case AIModelClientType.ResponsesApi:
            {
#pragma warning disable OPENAI001
                ChatClientAgent inner = client.GetOpenAIResponseClient(model.ModelName).CreateAIAgent();
#pragma warning restore OPENAI001
                return new Agent(inner);
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}