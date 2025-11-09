using OpenAI.Chat;

#pragma warning disable OPENAI001
// ReSharper disable CheckNamespace
namespace AgentFramework.Toolkit.AIAgents.Models;

public class ChatClientReasoning : AgentOptions
{
    public ChatReasoningEffortLevel ReasoningEffort { get; set; }
}