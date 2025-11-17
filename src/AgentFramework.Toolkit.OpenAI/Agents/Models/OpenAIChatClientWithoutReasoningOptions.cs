using AgentFramework.Toolkit.Agents.Models;

namespace AgentFramework.Toolkit.OpenAI.Agents.Models;

public class OpenAIChatClientWithoutReasoningOptions : AgentOptions
{
    public float? Temperature { get; set; }
}