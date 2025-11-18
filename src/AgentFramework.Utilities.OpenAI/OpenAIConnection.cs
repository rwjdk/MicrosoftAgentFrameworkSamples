using OpenAI;

namespace AgentFramework.Utilities.OpenAI;

public class OpenAIConnection
{
    public required string ApiKey { get; set; }

    public string? Endpoint { get; set; }

    public Action<OpenAIClientOptions>? AdditionalOpenAIClientOptions { get; set; }
}