using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;

#pragma warning disable MEAI001

public class AgUiHumanInTheLoopAgent(AIAgent innerAgent) : AIAgent
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

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (AgentResponseUpdate update in innerAgent.RunStreamingAsync(messages, session, options, cancellationToken))
        {
            foreach (FunctionApprovalRequestContent functionApprovalRequestContent in update.Contents.OfType<FunctionApprovalRequestContent>())
            {
                    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(functionApprovalRequestContent, JsonSerializerOptions.Web);
                    yield return new AgentResponseUpdate(ChatRole.Assistant, [new DataContent(bytes, "application/json")]);
            }

            yield return update;
        }
    }
}