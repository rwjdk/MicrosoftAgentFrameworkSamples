using GenerativeAI.Core;

// ReSharper disable once CheckNamespace
namespace MicrosoftAgentFramework.Toolkit.AIAgents;

public class GoogleAgentFactoryConfiguration
{
    public required IPlatformAdapter Adapter { get; set; }
}