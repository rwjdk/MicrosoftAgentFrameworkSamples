using Azure.AI.OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using AgentFramework.Toolkit.AIAgents.Models;

#pragma warning disable OPENAI001

// ReSharper disable once CheckNamespace
namespace AgentFramework.Toolkit.AIAgents;

public class AgentFactoryAzureOpenAI(AzureOpenAIAgentFactoryConfiguration configuration)
{
    public Agent CreateAgent(ResponsesApiNonReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, options, null, null);

        ChatClientAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(ApplyMiddleware(options, innerAgent));
        }

        return new Agent(innerAgent);
    }

    public Agent CreateAgent(ResponsesApiReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, options, null);

        AIAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(ApplyMiddleware(options, innerAgent));
        }

        return new Agent(innerAgent);
    }

    public Agent CreateAgent(ChatClientNonReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, options, null, null);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(ApplyMiddleware(options, innerAgent));
        }

        return new Agent(innerAgent);
    }

    public Agent CreateAgent(ChatClientReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, options);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(ApplyMiddleware(options, innerAgent));
        }

        return new Agent(innerAgent);
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

        return new AzureOpenAIClient(
            new Uri(configuration.Endpoint),
            new ApiKeyCredential(configuration.ApiKey!),
            azureOpenAIClientOptions);
    }

    private static AIAgent ApplyMiddleware(AgentOptions options, AIAgent innerAgent)
    {
        //todo - more middleware options
        if (options.RawToolCallDetails != null)
        {
            innerAgent = innerAgent.AsBuilder().Use(new ToolCallsHandler(options.RawToolCallDetails).ToolCallingMiddleware).Build();
        }

        return innerAgent;
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(AgentOptions options, NonReasoningAgentOptions? nonReasoningOptions, ResponsesApiReasoning? responsesApiReasoningOptions, ChatClientReasoning? chatClientReasoningOptions)
    {
        bool anyOptionsSet = false;
        ChatOptions chatOptions = new();
        if (options.Tools != null)
        {
            anyOptionsSet = true;
            chatOptions.Tools = options.Tools;
        }

        if (nonReasoningOptions?.Temperature != null)
        {
            anyOptionsSet = true;
            chatOptions.Temperature = nonReasoningOptions.Temperature;
        }

        if (responsesApiReasoningOptions != null)
        {
            if (responsesApiReasoningOptions.ReasoningEffort != null || responsesApiReasoningOptions.ReasoningSummaryVerbosity != null)
            {
                anyOptionsSet = true;
                chatOptions.RawRepresentationFactory = _ =>
                {
                    ResponseCreationOptions responseCreationOptions = new()
                    {
                        ReasoningOptions = new ResponseReasoningOptions
                        {
                            ReasoningEffortLevel = responsesApiReasoningOptions.ReasoningEffort,
                            ReasoningSummaryVerbosity = responsesApiReasoningOptions.ReasoningSummaryVerbosity
                        }
                    };
                    return responseCreationOptions;
                };
            }
        }

        if (chatClientReasoningOptions != null)
        {
            if (chatClientReasoningOptions.ReasoningEffort != null)
            {
                anyOptionsSet = true;
                chatOptions.RawRepresentationFactory = _ => new ChatCompletionOptions
                {
                    ReasoningEffortLevel = chatClientReasoningOptions.ReasoningEffort
                };
            }
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Instructions = options.Instructions,
            Description = options.Description,
            Id = options.Id,
        };
        if (anyOptionsSet)
        {
            chatClientAgentOptions.ChatOptions = chatOptions;
        }

        return chatClientAgentOptions;
    }
}