using AgentFramework.Toolkit.Agents.Models;

namespace AgentFramework.Toolkit.GoogleGenerativeAI.Agents.Models;

public class GoogleGenerativeAIOptions : AgentOptions
{
    public float? Temperature { get; set; }
}