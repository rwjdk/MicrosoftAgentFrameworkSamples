using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentUserInteraction.Advanced.Server.AgUiSpecializedAgents;

public class AgUiStructuredOutputAgent<T>(ChatClientAgent innerAgent) : AIAgent
{
    public override ValueTask<AgentThread> GetNewThreadAsync(CancellationToken cancellationToken = default)
    {
        return innerAgent.GetNewThreadAsync(cancellationToken);
    }

    public override ValueTask<AgentThread> DeserializeThreadAsync(JsonElement serializedThread, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.DeserializeThreadAsync(serializedThread, jsonSerializerOptions, cancellationToken);
    }

    protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("AG-UI Agents Always use streaming");
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ChatClientAgentResponse<T> jsonResponse = await innerAgent.RunAsync<T>(messages, thread, null, options, null, cancellationToken);
        yield return new AgentResponseUpdate(ChatRole.Assistant,
        [
            new TextContent(jsonResponse.Text)
        ]);
    }
}