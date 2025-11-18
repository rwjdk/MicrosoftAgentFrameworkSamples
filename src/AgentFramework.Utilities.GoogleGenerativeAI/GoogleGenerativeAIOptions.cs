namespace AgentFramework.Utilities.GoogleGenerativeAI;

public class GoogleGenerativeAIOptions : AgentOptions
{
    public float? Temperature { get; set; }

    /// <summary>
    /// How many tokens are allowed for thinking/reasoning
    /// </summary>
    public int ThinkingBudget { get; set; }
}