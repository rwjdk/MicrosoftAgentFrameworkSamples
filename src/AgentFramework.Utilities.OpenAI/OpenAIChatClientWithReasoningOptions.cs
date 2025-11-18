using OpenAI.Chat;

#pragma warning disable OPENAI001
namespace AgentFramework.Utilities.OpenAI;

public class OpenAIChatClientWithReasoningOptions : AgentOptions
{
    public ChatReasoningEffortLevel? ReasoningEffort { get; set; }
}