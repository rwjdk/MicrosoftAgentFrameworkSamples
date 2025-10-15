using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI;

namespace Workflow.AiAssisted.PizzaSample;

public class AgentFactory(Configuration configuration)
{
    public ChatClientAgent CreateOrderTakerAgent()
    {
        return CreateAzureOpenAiClient()
            .GetChatClient(configuration.ChatDeploymentName)
            .CreateAIAgent(instructions: "You are a Pizza Order Taker, parsing the customers order");
    }

    public ChatClientAgent CreateWarningToCustomerAgent()
    {
        return CreateAzureOpenAiClient()
            .GetChatClient(configuration.ChatDeploymentName)
            .CreateAIAgent(instructions: "You are a Pizza Confirmer. that need to explain to a user if a pizza order can't be met");
    }

    private AzureOpenAIClient CreateAzureOpenAiClient()
    {
        AzureOpenAIClient azureOpenAiClient = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));
        return azureOpenAiClient;
    }
}