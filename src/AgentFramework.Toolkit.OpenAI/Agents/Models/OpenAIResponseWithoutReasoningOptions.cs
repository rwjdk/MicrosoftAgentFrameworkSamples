using AgentFramework.Toolkit.Agents.Models;

namespace AgentFramework.Toolkit.OpenAI.Agents.Models;

public class OpenAIResponseWithoutReasoningOptions : AgentOptions
{
    public float? Temperature { get; set; }
}