namespace Shared;

public record Configuration(
    string Endpoint,
    string Key,
    string ChatDeploymentName,
    string EmbeddingModelName,
    string AzureAiFoundryAgentEndpoint,
    string AzureAiFoundryAgentId,
    string BingApiKey);