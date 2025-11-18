using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFramework.Utilities.OpenAI;

public class OpenAIResponseWithReasoningOptions : AgentOptions
{
    public ResponseReasoningEffortLevel? ReasoningEffort { get; set; }
    public ResponseReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }
}