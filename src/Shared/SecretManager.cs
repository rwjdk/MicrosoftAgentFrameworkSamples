using Microsoft.Extensions.Configuration;

namespace Shared;

public class SecretManager
{
    /* This SecretManager relies on .NET User Secrets in the following format
    ************************************************************************************************************************************************
    {
      "OpenAiApiKey": "todo",
      "AzureOpenAiEndpoint": "todo",
      "AzureOpenAiKey": "todo",
      "ChatDeploymentName": "todo",
      "EmbeddingModelName": "todo",
      "AzureAiFoundryAgentEndpoint" : "todo",
      "AzureAiFoundryAgentId" : "todo",
      "BingApiKey" : "todo",
      "HuggingFaceApiKey": "todo",
      "OpenRouterApiKet" : "todo",
      "OpenRouterApiKey" : "todo",
      "ApplicationInsightsConnectionString" : "todo",
      "GoogleGeminiApiKey" : "todo",
      "XAiGrokApiKey" : "todo",
      "TrelloApiKey" : "todo",
      "TrelloToken" : "todo",
      "AnthropicApiKey" : "todo",
      "MistralApiKey" : "todo",
      "AmazonBedrockApiKey" : "todo",
      "OpenWeatherApiKey" : "todo",
    }
    ************************************************************************************************************************************************
    - See the how-to guides on how to create your Azure Resources in the ReadMe
    - See https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets on how to work with user-secrets
    ************************************************************************************************************************************************
    */

    public static Secrets GetSecrets()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddUserSecrets<SecretManager>().Build();

        return new Secrets(
            configurationRoot["OpenAiApiKey"] ?? string.Empty,
            configurationRoot["AzureOpenAiEndpoint"] ?? string.Empty,
            configurationRoot["AzureOpenAiKey"] ?? string.Empty,
            configurationRoot["ChatDeploymentName"] ?? string.Empty,
            configurationRoot["EmbeddingModelName"] ?? string.Empty,
            configurationRoot["AzureAiFoundryAgentEndpoint"] ?? string.Empty,
            configurationRoot["AzureAiFoundryAgentId"] ?? string.Empty,
            configurationRoot["BingApiKey"] ?? string.Empty,
            configurationRoot["GitHubPatToken"] ?? string.Empty,
            configurationRoot["HuggingFaceApiKey"] ?? string.Empty,
            configurationRoot["OpenRouterApiKey"] ?? string.Empty,
            configurationRoot["ApplicationInsightsConnectionString"] ?? string.Empty,
            configurationRoot["GoogleGeminiApiKey"] ?? string.Empty,
            configurationRoot["XAiGrokApiKey"] ?? string.Empty,
            configurationRoot["TrelloApiKey"] ?? string.Empty,
            configurationRoot["TrelloToken"] ?? string.Empty,
            configurationRoot["AnthropicApiKey"] ?? string.Empty,
            configurationRoot["MistralApiKey"] ?? string.Empty,
            configurationRoot["AmazonBedrockApiKey"] ?? string.Empty,
            configurationRoot["OpenWeatherApiKey"] ?? string.Empty);
    }
}