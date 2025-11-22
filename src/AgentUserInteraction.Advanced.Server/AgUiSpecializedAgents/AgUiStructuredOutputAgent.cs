using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentUserInteraction.Advanced.Server.AgUiSpecializedAgents;

public class AgUiStructuredOutputAgent<T>(ChatClientAgent innerAgent) : AIAgent
{
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
        throw new NotImplementedException("AG-UI Agents Always use streaming");
    }

    public override async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ChatClientAgentRunResponse<T> jsonResponse = await innerAgent.RunAsync<T>(messages, thread, null, options, null, cancellationToken);
        yield return new AgentRunResponseUpdate(ChatRole.Assistant,
        [
            new TextContent(jsonResponse.Text)
        ]);
    }
}