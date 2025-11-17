using System.ClientModel;
using System.ClientModel.Primitives;
using AgentFramework.Toolkit.Agents;
using AgentFramework.Toolkit.Agents.Models;
using AgentFramework.Toolkit.OpenAI.Agents.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFramework.Toolkit.OpenAI.Agents;

public class AgentFactoryOpenAI(OpenAIConnection connection)
{
    public Agent CreateAgent(OpenAIResponseWithoutReasoningOptions options)
    {
        OpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, options, null, null);

        ChatClientAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.OpenAIResponses);
        }

        return new Agent(innerAgent, Provider.OpenAIResponses);
    }

    public Agent CreateAgent(OpenAIResponseWithReasoningOptions options)
    {
        OpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, options, null);

        AIAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.OpenAIResponses);
        }

        return new Agent(innerAgent, Provider.OpenAIResponses);
    }

    public Agent CreateAgent(OpenAIChatClientWithoutReasoningOptions options)
    {
        OpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, options, null, null, null);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.OpenAIChatClient);
        }

        return new Agent(innerAgent, Provider.OpenAIChatClient);
    }

    public Agent CreateAgent(OpenAIChatClientWithReasoningOptions options)
    {
        OpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, null, options);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new Agent(options.ApplyMiddleware(innerAgent), Provider.OpenAIChatClient);
        }

        return new Agent(innerAgent, Provider.OpenAIChatClient);
    }

    private OpenAIClient CreateClient(AgentOptions options)
    {
        OpenAIClientOptions openAIClientOptions = new()
        {
            NetworkTimeout = options.NetworkTimeout
        };

        if (!string.IsNullOrWhiteSpace(connection.Endpoint))
        {
            openAIClientOptions.Endpoint = new Uri(connection.Endpoint);
        }

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
            openAIClientOptions.Transport = new HttpClientPipelineTransport(inspectingHttpClient);
        }

        connection.AdditionalOpenAIClientOptions?.Invoke(openAIClientOptions);

        return new OpenAIClient(new ApiKeyCredential(connection.ApiKey), openAIClientOptions);
    }

    public static ChatClientAgentOptions CreateChatClientAgentOptions(AgentOptions options, OpenAIChatClientWithoutReasoningOptions? chatClientWithoutReasoning, OpenAIResponseWithoutReasoningOptions? responseWithoutReasoning, OpenAIResponseWithReasoningOptions? responsesApiReasoningOptions, OpenAIChatClientWithReasoningOptions? chatClientReasoningOptions)
    {
        bool anyOptionsSet = false;
        ChatOptions chatOptions = new();
        if (options.Tools != null)
        {
            anyOptionsSet = true;
            chatOptions.Tools = options.Tools;
        }
        
        if (options.MaxOutputTokens.HasValue)
        {
            anyOptionsSet = true;
            chatOptions.MaxOutputTokens = options.MaxOutputTokens.Value;
        }

        if (chatClientWithoutReasoning?.Temperature != null)
        {
            anyOptionsSet = true;
            chatOptions.Temperature = chatClientWithoutReasoning.Temperature;
        }

        if (responsesApiReasoningOptions != null)
        {
            if (responsesApiReasoningOptions.ReasoningEffort != null || responsesApiReasoningOptions.ReasoningSummaryVerbosity.HasValue)
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

        if (chatClientReasoningOptions?.ReasoningEffort != null)
        {
            anyOptionsSet = true;
            chatOptions.RawRepresentationFactory = _ => new ChatCompletionOptions
            {
                ReasoningEffortLevel = chatClientReasoningOptions.ReasoningEffort
            };
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

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }
}