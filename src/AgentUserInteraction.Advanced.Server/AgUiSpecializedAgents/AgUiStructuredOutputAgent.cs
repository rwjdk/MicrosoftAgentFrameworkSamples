using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentUserInteraction.Advanced.Server.AgUiSpecializedAgents;

public class AgUiStructuredOutputAgent<T>(ChatClientAgent innerAgent) : AIAgent
{
    public override ValueTask<AgentSession> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        return innerAgent.CreateSessionAsync(cancellationToken);
    }

    public override JsonElement SerializeSession(AgentSession session, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return innerAgent.SerializeSession(session, jsonSerializerOptions);
    }

    public override ValueTask<AgentSession> DeserializeSessionAsync(JsonElement serializedSession, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.DeserializeSessionAsync(serializedSession, jsonSerializerOptions, cancellationToken);
    }

    protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return RunStreamingAsync(messages, session, options, cancellationToken).ToAgentResponseAsync(cancellationToken);
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ChatClientAgentResponse<T> jsonResponse = await innerAgent.RunAsync<T>(messages, session, null, options, null, cancellationToken);
        yield return new AgentResponseUpdate(ChatRole.Assistant,
        [
            new TextContent(jsonResponse.Text)
        ]);
    }
}