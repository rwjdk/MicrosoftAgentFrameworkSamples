using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MicrosoftAgentFramework.Utilities.Extensions;

// ReSharper disable once InconsistentNaming
public static class AIAgentExtensions
{
    public static async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(
        this AIAgent agent,
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        if (agent is ChatClientAgent chatClientAgent)
        {
            return await chatClientAgent.RunAsync<T>(thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
        }

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new JsonStringEnumConverter() }
        };

        if (options != null)
        {
            if (options is ChatClientAgentRunOptions { ChatOptions: not null } chatClientAgentRunOptions)
            {
                chatClientAgentRunOptions.ChatOptions.ResponseFormat = ChatResponseFormat.ForJsonSchema<T>(jsonSerializerOptions);
            }
            else
            {
                throw new NotSupportedException("Structure Output is not possible in this scenario");
            }
        }
        else
        {
            options = new ChatClientAgentRunOptions
            {
                ChatOptions = new()
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema<T>(jsonSerializerOptions)
                }
            };
        }

        Type agentType = agent.GetType();

        //FunctionInvocationDelegatingAgent (which is internal so need to be called using dynamic reflection)
        if (agentType.Name == "FunctionInvocationDelegatingAgent")
        {
            dynamic functionInvocationDelegatingAgent = agent;
            AgentRunResponse responseFromFunctionInvocationDelegatingAgent = await functionInvocationDelegatingAgent.RunAsync(messages, thread, options, cancellationToken);
            return new ChatClientAgentRunResponse<T>(new ChatResponse<T>(responseFromFunctionInvocationDelegatingAgent.AsChatResponse(), jsonSerializerOptions));
        }

        //Normal other agent (Lets try and see if it works)
        AgentRunResponse response = await agent.RunAsync(messages, thread, options, cancellationToken);
        return new ChatClientAgentRunResponse<T>(new ChatResponse<T>(response.AsChatResponse(), jsonSerializerOptions));
    }

    private static readonly Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>> _delegateFunc;

    private sealed class MiddlewareEnabledFunction(AIAgent innerAgent, AIFunction innerFunction, Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>> next) : DelegatingAIFunction(innerFunction)
    {
        protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            return null;
        }
    }

    public static async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(
        this AIAgent agent,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default) =>
        await agent.RunAsync<T>([], thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);

    public static async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(
        this AIAgent agent,
        string message,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        return await agent.RunAsync<T>(new ChatMessage(ChatRole.User, message), thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
    }

    public static async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(
        this AIAgent agent,
        ChatMessage message,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        return await agent.RunAsync<T>([message], thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
    }
}