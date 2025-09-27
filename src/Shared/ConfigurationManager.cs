using Microsoft.Extensions.Configuration;

namespace Shared;

public class ConfigurationManager
{
    /* This ConfigurationManager relies on .NET User Secrets in the following format
    ************************************************************************************************************************************************
    {
      "Endpoint": "todo", //URL of your Azure OpenAI Service
      "Key": "todo", //Key of your Azure OpenAI Service
      "ChatDeploymentName": "todo", //DeploymentName of your Azure OpenAI Chat-model (example: "gpt-4o-mini")
      "EmbeddingModelName": "todo", //[Optional] Embedding Model for RAG (example: "text-embedding-ada-002")
      "AzureAiFoundryAgentEndpoint" : "todo", //[Optional] Endpoint for the Azure AI Foundry Agents (if you wish to test those demos)
      "AzureAiFoundryAgentId" : "todo", //[Optional] ID of your agent for the Azure AI Foundry Agents (if you wish to test those demos)
      "BingApiKey" : "todo" //[OPTIONAL] If you wish to use BingSearch in AI Agents
    }
    ************************************************************************************************************************************************
    - See the how-to guides on how to create your Azure Resources in the ReadMe
    - See https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets on how to work with user-secrets
    ************************************************************************************************************************************************
    */

    public static Configuration GetConfiguration()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddUserSecrets<ConfigurationManager>().Build();
        Exception notSetupException = new("It seems you have not yet set up you ConfigurationManager in the Shared Project. Please go there to do so");
        string endpoint = configurationRoot["Endpoint"] ?? throw notSetupException;
        string key = configurationRoot["Key"] ?? throw notSetupException;
        string chatDeploymentName = configurationRoot["ChatDeploymentName"] ?? throw notSetupException;
        string embeddingModelName = configurationRoot["EmbeddingModelName"] ?? string.Empty;
        string azureAiFoundryAgentEndpoint = configurationRoot["AzureAiFoundryAgentEndpoint"] ?? string.Empty;
        string azureAiFoundryAgentId = configurationRoot["AzureAiFoundryAgentId"] ?? string.Empty;
        string bingApiKey = configurationRoot["BingApiKey"] ?? string.Empty;

        return new Configuration(endpoint, key, chatDeploymentName, embeddingModelName, azureAiFoundryAgentEndpoint, azureAiFoundryAgentId, bingApiKey);
    }
}