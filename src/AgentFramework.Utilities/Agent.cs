using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFramework.Utilities;

public class Agent(AIAgent innerAgent, AgentProvider provider) : AIAgent
{
    public AgentProvider Provider { get; set; } = provider;
    public AIAgent InnerAgent => innerAgent;
    public override string Id => innerAgent.Id;
    public override string? Name => innerAgent.Name;
    public override string? Description => innerAgent.Description;
    public override string DisplayName => innerAgent.DisplayName;

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        return innerAgent.GetService(serviceType, serviceKey);
    }

    public override bool Equals(object? obj)
    {
        return innerAgent.Equals(obj);
    }

    public override int GetHashCode()
    {
        return innerAgent.GetHashCode();
    }

    public override string? ToString()
    {
        return innerAgent.ToString();
    }

    public override AgentThread GetNewThread()
    {
        return innerAgent.GetNewThread();
    }

    public override AgentThread DeserializeThread(JsonElement serializedThread, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return innerAgent.DeserializeThread(serializedThread, jsonSerializerOptions);
    }

    public override Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunAsync(messages, thread, options, cancellationToken);
    }

    public override IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken);
    }

    public async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        if (innerAgent is ChatClientAgent chatClientAgent)
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

        //Normal other agent (Lets try and see if it works)
        AgentRunResponse response = await innerAgent.RunAsync(messages, thread, options, cancellationToken);
        return new ChatClientAgentRunResponse<T>(new ChatResponse<T>(response.AsChatResponse(), jsonSerializerOptions));
    }

    public async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default) =>
        await RunAsync<T>([], thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);

    public async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(
        string message,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        return await RunAsync<T>(new ChatMessage(ChatRole.User, message), thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
    }

    public async Task<ChatClientAgentRunResponse<T>> RunAsync<T>(
        ChatMessage message,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        return await RunAsync<T>([message], thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
    }
}