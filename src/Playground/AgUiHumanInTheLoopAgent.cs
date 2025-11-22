using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;

#pragma warning disable MEAI001

public class AgUiHumanInTheLoopAgent(AIAgent innerAgent) : AIAgent
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
        return RunStreamingAsync(messages, thread, options, cancellationToken).ToAgentRunResponseAsync(cancellationToken);
    }

    public override async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (AgentRunResponseUpdate update in innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken))
        {
            foreach (UserInputRequestContent inputRequest in update.UserInputRequests)
            {
                if (inputRequest is FunctionApprovalRequestContent functionApprovalRequestContent)
                {
                    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(functionApprovalRequestContent, JsonSerializerOptions.Web);
                    yield return new AgentRunResponseUpdate(ChatRole.Assistant, [new DataContent(bytes, "application/json")]);
                }
            }

            yield return update;
        }
    }
}