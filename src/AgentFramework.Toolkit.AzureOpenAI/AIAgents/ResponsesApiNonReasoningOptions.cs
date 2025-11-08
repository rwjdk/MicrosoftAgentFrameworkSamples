using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFramework.Toolkit.AzureOpenAI.AIAgents;

public abstract class Options
{
    public required string DeploymentModelName { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public IList<AITool>? Tools { get; set; }
    public TimeSpan? NetworkTimeout { get; set; }
    public Action<RawCallDetails>? RawHttpCallDetails { get; set; }
    public Action<ToolCallingDetails>? RawToolCallDetails { get; set; }
}

public abstract class NonReasoningOptions : Options
{
    public float? Temperature { get; set; }
}

public class ResponsesApiNonReasoningOptions : NonReasoningOptions;

public class ResponsesApiReasoningOptions : Options
{
    public ResponseReasoningEffortLevel ReasoningEffort { get; set; }
    public ResponseReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }
}

public class ChatClientNonReasoningOptions : NonReasoningOptions;

public class ChatClientReasoningOptions : Options
{
    public ChatReasoningEffortLevel ReasoningEffort { get; set; }
}