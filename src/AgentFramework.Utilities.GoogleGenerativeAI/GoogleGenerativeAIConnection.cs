using GenerativeAI.Core;

namespace AgentFramework.Utilities.GoogleGenerativeAI;

public class GoogleGenerativeAIConnection
{
    public string? ApiKey { get; set; }
    public IPlatformAdapter? Adapter { get; set; }
}