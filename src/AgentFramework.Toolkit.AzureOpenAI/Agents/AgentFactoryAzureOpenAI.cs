using System.ClientModel;
using System.ClientModel.Primitives;
using AgentFramework.Toolkit.Agents;
using AgentFramework.Toolkit.Agents.Models;
using AgentFramework.Toolkit.OpenAI.Agents;
using AgentFramework.Toolkit.OpenAI.Agents.Models;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;

#pragma warning disable OPENAI001

namespace AgentFramework.Toolkit.AzureOpenAI.Agents;

public class AgentFactoryAzureOpenAI(AzureOpenAIConnection connection)
{
    public Agent CreateAgent(OpenAIResponseWithoutReasoningOptions options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = AgentFactoryOpenAI.CreateChatClientAgentOptions(options, null, options, null, null);

        ChatClientAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.AzureOpenAIResponses);
        }

        return new Agent(innerAgent, Provider.AzureOpenAIResponses);
    }

    public Agent CreateAgent(OpenAIResponseWithReasoningOptions options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = AgentFactoryOpenAI.CreateChatClientAgentOptions(options, null, null, options, null);

        AIAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.AzureOpenAIResponses);
        }

        return new Agent(innerAgent, Provider.AzureOpenAIResponses);
    }

    public Agent CreateAgent(OpenAIChatClientWithoutReasoningOptions options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = AgentFactoryOpenAI.CreateChatClientAgentOptions(options, options, null, null, null);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.AzureOpenAIChatClient);
        }

        return new Agent(innerAgent, Provider.AzureOpenAIChatClient);
    }

    public Agent CreateAgent(OpenAIChatClientWithReasoningOptions options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = AgentFactoryOpenAI.CreateChatClientAgentOptions(options, null, null, null, options);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.AzureOpenAIChatClient);
        }

        return new Agent(innerAgent, Provider.AzureOpenAIChatClient);
    }

    private AzureOpenAIClient CreateClient(AgentOptions options)
    {
        //todo - support RBAC
        AzureOpenAIClientOptions azureOpenAIClientOptions = new()
        {
            NetworkTimeout = options.NetworkTimeout
        };

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
            azureOpenAIClientOptions.Transport = new HttpClientPipelineTransport(inspectingHttpClient);
        }

        connection.AdditionalAzureOpenAIClientOptions?.Invoke(azureOpenAIClientOptions);

        return new AzureOpenAIClient(new Uri(connection.Endpoint), new ApiKeyCredential(connection.ApiKey!), azureOpenAIClientOptions);
    }
}