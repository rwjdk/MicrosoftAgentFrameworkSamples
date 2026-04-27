using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;

namespace Shared;

public class SecretsManager
{
    /* This SecretsManager relies on .NET User Secrets in the following format
    ************************************************************************************************************************************************
    {
      "OpenAiApiKey": "<value>",
      "AzureOpenAiEndpoint": "<value>",
      "AzureOpenAiKey": "<value>",
      "AzureAiFoundryAgentEndpoint" : "<value>",
      "AzureAiFoundryAgentId" : "<value>",
      "BingApiKey" : "<value>",
      "HuggingFaceApiKey": "<value>",
      "OpenRouterApiKet" : "<value>",
      "OpenRouterApiKey" : "<value>",
      "ApplicationInsightsConnectionString" : "<value>",
      "GoogleGeminiApiKey" : "<value>",
      "XAiGrokApiKey" : "<value>",
      "TrelloApiKey" : "<value>",
      "TrelloToken" : "<value>",
      "AnthropicApiKey" : "<value>",
      "MistralApiKey" : "<value>",
      "AmazonBedrockApiKey" : "<value>",
      "OpenWeatherApiKey" : "<value>",
    }
    ************************************************************************************************************************************************
    - See the how-to guides on how to create your Azure Resources in the ReadMe
    - See https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets on how to work with user-secrets
    ************************************************************************************************************************************************
    */

    public static Secrets GetSecrets()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddUserSecrets<SecretsManager>().Build();

        return new Secrets(
            configurationRoot["OpenAiApiKey"] ?? string.Empty,
            configurationRoot["AzureOpenAiEndpoint"] ?? string.Empty,
            configurationRoot["AzureOpenAiKey"] ?? string.Empty,
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

    public static (Uri endpoint, ApiKeyCredential apiKey) GetAzureOpenAICredentials(bool newUriFormat)
    {
        //New format (https://<name>.openai.azure.com/openai/v1)
        //Old format (https://<name>.openai.azure.com/)
        string suffix = "openai/v1";
        Secrets secrets = GetSecrets();
        string secretEndpoint = secrets.AzureOpenAiEndpoint;
        if (!newUriFormat)
        {
            if (secretEndpoint.EndsWith(suffix))
            {
                //Azure OpenAI Client can't handle getting the new format so lets strip that
                secretEndpoint = secretEndpoint[..^suffix.Length];
            }
            return (new Uri(secretEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
        }
        
        if (secretEndpoint.EndsWith(suffix))
        {
            return (new Uri(secretEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
        }

        if (!secretEndpoint.EndsWith('/'))
        {
            secretEndpoint += "/";
        }
        return (new Uri(secretEndpoint + suffix), new ApiKeyCredential(secrets.AzureOpenAiKey));

    }

    public static string GetOpenAICredentials()
    {
        Secrets secrets = GetSecrets();
        return secrets.OpenAiApiKey;
    }
}