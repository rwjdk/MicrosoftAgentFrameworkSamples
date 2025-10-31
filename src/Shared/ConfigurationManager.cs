using Microsoft.Extensions.Configuration;

namespace Shared;

public class ConfigurationManager
{
    /* This ConfigurationManager relies on .NET User Secrets in the following format
    ************************************************************************************************************************************************
    {
      "OpenAiApiKey": "todo", //URL of your OpenAI API Key
      "AzureOpenAiEndpoint": "todo", //URL of your Azure OpenAI Service
      "AzureOpenAiKey": "todo", //Key of your Azure OpenAI Service
      "ChatDeploymentName": "todo", //DeploymentName of your Azure OpenAI Chat-model (example: "gpt-4o-mini")
      "EmbeddingModelName": "todo", //[Optional] Embedding Model for RAG (example: "text-embedding-ada-002")
      "AzureAiFoundryAgentEndpoint" : "todo", //[Optional] Endpoint for the Azure AI Foundry Agents (if you wish to test those demos)
      "AzureAiFoundryAgentId" : "todo", //[Optional] ID of your agent for the Azure AI Foundry Agents (if you wish to test those demos)
      "BingApiKey" : "todo",
      "HuggingFaceApiKey": "todo",
      "OpenRouterApiKet" : "todo",
      "OpenRouterApiKey" : "todo",
      "ApplicationInsightsConnectionString" : "todo",
      "GoogleGeminiApiKey" : "todo",
      "XAiGrokApiKey" : "todo",
    }
    ************************************************************************************************************************************************
    - See the how-to guides on how to create your Azure Resources in the ReadMe
    - See https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets on how to work with user-secrets
    ************************************************************************************************************************************************
    */

    public static Configuration GetConfiguration()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddUserSecrets<ConfigurationManager>().Build();
        string openAiApiKey = configurationRoot["OpenAiApiKey"] ?? string.Empty;
        string azureOpenAiEndpoint = configurationRoot["AzureOpenAiEndpoint"] ?? string.Empty;
        string azureOpenAiKey = configurationRoot["AzureOpenAiKey"] ?? string.Empty;
        string chatDeploymentName = configurationRoot["ChatDeploymentName"] ?? string.Empty;
        string embeddingModelName = configurationRoot["EmbeddingModelName"] ?? string.Empty;
        string azureAiFoundryAgentEndpoint = configurationRoot["AzureAiFoundryAgentEndpoint"] ?? string.Empty;
        string azureAiFoundryAgentId = configurationRoot["AzureAiFoundryAgentId"] ?? string.Empty;
        string bingApiKey = configurationRoot["BingApiKey"] ?? string.Empty;
        string githubPatToken = configurationRoot["GitHubPatToken"] ?? string.Empty;
        string huggingFaceApiKey = configurationRoot["HuggingFaceApiKey"] ?? string.Empty;
        string openRouterApiKey = configurationRoot["OpenRouterApiKey"] ?? string.Empty;
        string applicationInsightsConnectionString = configurationRoot["ApplicationInsightsConnectionString"] ?? string.Empty;
        string googleGeminiApiKey = configurationRoot["GoogleGeminiApiKey"] ?? string.Empty;
        string xAiGrokApiKey = configurationRoot["XAiGrokApiKey"] ?? string.Empty;

        return new Configuration(
            openAiApiKey,
            azureOpenAiEndpoint,
            azureOpenAiKey,
            chatDeploymentName,
            embeddingModelName,
            azureAiFoundryAgentEndpoint,
            azureAiFoundryAgentId,
            bingApiKey,
            githubPatToken,
            huggingFaceApiKey,
            openRouterApiKey,
            applicationInsightsConnectionString,
            googleGeminiApiKey,
            xAiGrokApiKey);
    }
}