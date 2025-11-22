using System.Runtime.CompilerServices;
using System.Text.Json;
using AgentUserInteraction.Advanced.SharedModels;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentUserInteraction.Advanced.Server.AgUiSpecializedAgents;

public class AgUiStructuredToolsOutputAgent(ChatClientAgent innerAgent, string toolCallToReportBackAsContent) : AIAgent
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
        // Track function calls that should trigger state events
        Dictionary<string, FunctionCallContent> trackedFunctionCalls = [];

        await foreach (AgentRunResponseUpdate update in innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken).ConfigureAwait(false))
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
                yield return new AgentRunResponseUpdate(
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