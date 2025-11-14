using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace MicrosoftAgentFramework.Utilities.Extensions;

[PublicAPI]
public static class AIAgentExtensions
{
    extension(AIAgent agent)
    {
        public async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(IEnumerable<ChatMessage> messages,
            AgentThread? thread = null,
            JsonSerializerOptions? serializerOptions = null,
            AgentRunOptions? options = null,
            bool? useJsonSchemaResponseFormat = null,
            CancellationToken cancellationToken = default)
        {
            if (agent is ChatClientAgent chatClientAgent)
            {
                return await chatClientAgent.RunAsync<T>(messages, thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
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

        public async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(AgentThread? thread = null,
            JsonSerializerOptions? serializerOptions = null,
            AgentRunOptions? options = null,
            bool? useJsonSchemaResponseFormat = null,
            CancellationToken cancellationToken = default) =>
            await agent.RunAsync<T>([], thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);

        public async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(string message,
            AgentThread? thread = null,
            JsonSerializerOptions? serializerOptions = null,
            AgentRunOptions? options = null,
            bool? useJsonSchemaResponseFormat = null,
            CancellationToken cancellationToken = default)
        {
            return await agent.RunAsync<T>(new ChatMessage(ChatRole.User, message), thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
        }

        public async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(ChatMessage message,
            AgentThread? thread = null,
            JsonSerializerOptions? serializerOptions = null,
            AgentRunOptions? options = null,
            bool? useJsonSchemaResponseFormat = null,
            CancellationToken cancellationToken = default)
        {
            return await agent.RunAsync<T>([message], thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
        }
    }
}