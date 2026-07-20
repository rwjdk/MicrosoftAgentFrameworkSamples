using System.Text.Json.Serialization;

namespace DoItYourselfAiCalling.Models;

internal class MyMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public IList<MyMessageToolCallRequest>? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
}

internal class MyMessageToolCallRequest
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("function")]
    public required Function Function { get; set; }
}

internal class Function
{
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}