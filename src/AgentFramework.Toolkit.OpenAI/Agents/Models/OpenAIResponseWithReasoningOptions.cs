using AgentFramework.Toolkit.Agents.Models;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFramework.Toolkit.OpenAI.Agents.Models;

public class OpenAIResponseWithReasoningOptions : AgentOptions
{
    public ResponseReasoningEffortLevel? ReasoningEffort { get; set; }
    public ResponseReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }
}