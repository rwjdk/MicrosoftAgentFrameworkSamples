using AgentFramework.Toolkit.Agents.Models;
using OpenAI.Chat;

#pragma warning disable OPENAI001
namespace AgentFramework.Toolkit.OpenAI.Agents.Models;

public class OpenAIChatClientWithReasoningOptions : AgentOptions
{
    public ChatReasoningEffortLevel? ReasoningEffort { get; set; }
}