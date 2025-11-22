using System.Text.Json;

namespace AgentUserInteraction.Advanced.SharedModels;

public class MovieResult
{
    public required string Title { get; set; }
    public required int YearOfRelease { get; set; }
    public required string Director { get; set; }

    public static MovieResult FromJson(string json)
    {
        return JsonSerializer.Deserialize<MovieResult>(json, JsonSerializerOptions.Web)!;
    }
}