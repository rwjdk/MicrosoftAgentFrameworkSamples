using System.Runtime.CompilerServices;
using System.Text.Json;
using AgentUserInteraction.Advanced.SharedModels;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentUserInteraction.Advanced.Server.AgUiSpecializedAgents;

public class AgUiStructuredToolsOutputAgent(ChatClientAgent innerAgent, string toolCallToReportBackAsContent) : AIAgent
{
    public override ValueTask<AgentSession> GetNewSessionAsync(CancellationToken cancellationToken = default)
    {
        return innerAgent.GetNewSessionAsync(cancellationToken);
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
        // Track function calls that should trigger state events
        Dictionary<string, FunctionCallContent> trackedFunctionCalls = [];

        await foreach (AgentResponseUpdate update in innerAgent.RunStreamingAsync(messages, session, options, cancellationToken).ConfigureAwait(false))
        {
            // Process contents: track function calls and emit state events for results
            List<AIContent> stateEventsToEmit = [];
            foreach (AIContent content in update.Contents)
            {
                if (content is FunctionCallContent callContent)
                {
                    if (callContent.Name == toolCallToReportBackAsContent)
                    {
                        trackedFunctionCalls[callContent.CallId] = callContent;
                        break;
                    }
                }
                else if (content is FunctionResultContent resultContent)
                {
                    // Check if this result matches a tracked function call
                    if (trackedFunctionCalls.TryGetValue(resultContent.CallId, out FunctionCallContent? matchedCall))
                    {
                        JsonElement jsonElement = (JsonElement)resultContent.Result!;
                        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(jsonElement, JsonSerializerOptions.Web);

                        // Determine event type based on the function name
                        if (matchedCall.Name == toolCallToReportBackAsContent)
                        {
                            stateEventsToEmit.Add(new DataContent(bytes, "application/json"));
                        }
                    }
                }
            }

            yield return update;

            if (stateEventsToEmit.Count > 0)
            {
                yield return new AgentResponseUpdate(
                    new ChatResponseUpdate(role: ChatRole.System, stateEventsToEmit)
                    {
                        MessageId = "delta_" + Guid.NewGuid().ToString("N"),
                        CreatedAt = update.CreatedAt,
                        ResponseId = update.ResponseId,
                        AuthorName = update.AuthorName,
                        Role = update.Role,
                        AdditionalProperties = update.AdditionalProperties,
                    })
                {
                    AgentId = update.AgentId
                };
            }
        }
    }
}