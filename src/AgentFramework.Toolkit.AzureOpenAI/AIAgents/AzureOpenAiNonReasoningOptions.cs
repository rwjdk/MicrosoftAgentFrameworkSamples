using OpenAI.Chat;

namespace AgentFramework.Toolkit.AzureOpenAI.AIAgents;

public class AzureOpenAiNonReasoningOptions : AzureOpenAiOptions
{
}

public class AzureOpenAiReasoningOptions : AzureOpenAiOptions
{
#pragma warning disable OPENAI001
    public ChatReasoningEffortLevel ReasoningEffort { get; set; }
#pragma warning restore OPENAI001
}

public abstract class AzureOpenAiOptions
{
    public TimeSpan? NetworkTimeout { get; set; }
}