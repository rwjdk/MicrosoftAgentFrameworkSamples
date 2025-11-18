using OpenAI;

namespace AgentFramework.Utilities.Grok;

public class GrokConnection
{
    public required string ApiKey { get; set; }

    public Action<OpenAIClientOptions>? AdditionalOpenAIClientOptions { get; set; }
}