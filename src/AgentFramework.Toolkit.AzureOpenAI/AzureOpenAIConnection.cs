using Azure.AI.OpenAI;

namespace AgentFramework.Toolkit.AzureOpenAI;

public class AzureOpenAIConnection
{
    public required string Endpoint { get; set; }

    public string? ApiKey { get; set; }

    public Action<AzureOpenAIClientOptions>? AdditionalAzureOpenAIClientOptions { get; set; }

    //todo - Support RBAC
}