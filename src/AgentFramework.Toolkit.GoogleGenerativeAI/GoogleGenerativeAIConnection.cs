using GenerativeAI.Core;

namespace AgentFramework.Toolkit.GoogleGenerativeAI;

public class GoogleGenerativeAIConnection
{
    public string? ApiKey { get; set; }
    public IPlatformAdapter? Adapter { get; set; }
}