using GenerativeAI.Core;

// ReSharper disable once CheckNamespace
namespace AgentFramework.Toolkit.AIAgents;

public class GoogleAgentFactoryConfiguration
{
    public required IPlatformAdapter Adapter { get; set; }
}