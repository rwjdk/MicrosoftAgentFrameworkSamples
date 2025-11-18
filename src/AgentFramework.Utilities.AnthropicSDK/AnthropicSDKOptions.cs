namespace AgentFramework.Utilities.AnthropicSDK;

public class AnthropicSDKOptions : AgentOptions
{
    public new required int MaxOutputTokens { get; set; }
    public float? Temperature { get; set; }

    /// <summary>
    /// Reasoning effort knob, in tokens. Higher value --> more internal reasoning.
    /// </summary>
    public int BudgetTokens { get; set; }

    /// <summary>
    /// If you want the advanced "interleaved thinking" mode where the thinking budget can exceed MaxOutputTokens
    /// </summary>
    public bool UseInterleavedThinking { get; set; }
}