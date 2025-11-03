using System;

// ReSharper disable once CheckNamespace
namespace MicrosoftAgentFramework.Toolkit.AIModels;

public class AzureOpenAIModel(string modelName, AIModelClientType clientType) : AIModel(modelName)
{
    public string? ReasoningEffort { get; set; }
    public TimeSpan? NetworkTimeout { get; set; }
    public AIModelClientType ClientType { get; set; } = clientType;

    public static AzureOpenAIModel ChatClientNonReasoningModel(string model, double? temperature)
    {
        return new AzureOpenAIModel(model, AIModelClientType.ChatClient)
        {
            Temperature = temperature
        };
    }

    public static AzureOpenAIModel ChatClientReasoningModel(string modelName, string? reasoningEffort = null, TimeSpan? networkTimeout = null)
    {
        return new AzureOpenAIModel(modelName, AIModelClientType.ChatClient)
        {
            ReasoningEffort = reasoningEffort,
            NetworkTimeout = networkTimeout,
        };
    }

    public static AzureOpenAIModel ResponsesApiNonReasoningModel(string modelName, double? temperature)
    {
        return new AzureOpenAIModel(modelName, AIModelClientType.ResponsesApi)
        {
            Temperature = temperature
        };
    }

    public static AzureOpenAIModel ResponsesApiReasoningModel(string modelName, string? reasoningEffort = null, TimeSpan? timeout = null)
    {
        return new AzureOpenAIModel(modelName, AIModelClientType.ResponsesApi)
        {
            ReasoningEffort = reasoningEffort
        };
    }
}

public enum AIModelProvider
{
}