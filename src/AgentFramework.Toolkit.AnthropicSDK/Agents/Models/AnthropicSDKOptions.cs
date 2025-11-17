using AgentFramework.Toolkit.Agents.Models;

namespace AgentFramework.Toolkit.AnthropicSDK.Agents.Models;

public class AnthropicSDKOptions : AgentOptions
{
    public new required int MaxOutputTokens { get; set; }
    public float? Temperature { get; set; }
}