// ReSharper disable once CheckNamespace

using Azure.AI.OpenAI;

// ReSharper disable once CheckNamespace
namespace MicrosoftAgentFramework.Toolkit.AIAgents;

public class AzureOpenAIAgentFactoryConfiguration
{
    public required string Endpoint { get; set; }

    public string? ApiKey { get; set; }

    public AzureOpenAIClientOptions? AzureOpenAIClientOptions { get; set; }

    //todo - Support RBAC
}