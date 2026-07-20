using System.Text.Json.Serialization;

namespace DoItYourselfAiCalling.Models;

internal class MyMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}