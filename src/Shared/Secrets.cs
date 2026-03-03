namespace Shared;

public record Secrets(
    string OpenAiApiKey,
    string AzureOpenAiEndpoint,
    string AzureOpenAiKey,
    string AzureAiFoundryAgentEndpoint,
    string AzureAiFoundryAgentId,
    string BingApiKey,
    string GitHubPatToken,
    string HuggingFaceApiKey,
    string OpenRouterApiKey,
    string ApplicationInsightsConnectionString,
    string GoogleGeminiApiKey,
    string XAiGrokApiKey,
    string TrelloApiKey,
    string TrelloToken,
    string AnthropicApiKey,
    string MistralApiKey,
    string AmazonBedrockApiKey,
    string OpenWeatherApiKey);