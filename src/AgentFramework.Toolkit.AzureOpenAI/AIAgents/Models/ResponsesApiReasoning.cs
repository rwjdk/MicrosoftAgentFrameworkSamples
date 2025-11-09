using OpenAI.Responses;
#pragma warning disable OPENAI001

// ReSharper disable CheckNamespace
using AgentFramework.Toolkit.AIAgents.Models;

public class ResponsesApiReasoning : AgentOptions
{
    public ResponseReasoningEffortLevel ReasoningEffort { get; set; }
    public ResponseReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }
}