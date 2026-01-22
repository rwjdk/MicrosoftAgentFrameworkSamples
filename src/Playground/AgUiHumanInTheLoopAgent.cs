using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;

#pragma warning disable MEAI001

public class AgUiHumanInTheLoopAgent(AIAgent innerAgent) : AIAgent
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
        return RunStreamingAsync(messages, thread, options, cancellationToken).ToAgentResponseAsync(cancellationToken);
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (AgentResponseUpdate update in innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken))
        {
            foreach (UserInputRequestContent inputRequest in update.UserInputRequests)
            {
                if (inputRequest is FunctionApprovalRequestContent functionApprovalRequestContent)
                {
                    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(functionApprovalRequestContent, JsonSerializerOptions.Web);
                    yield return new AgentResponseUpdate(ChatRole.Assistant, [new DataContent(bytes, "application/json")]);
                }
            }

            yield return update;
        }
    }
}