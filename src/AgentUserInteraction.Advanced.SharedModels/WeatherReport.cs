using System.Text.Json;

namespace AgentUserInteraction.Advanced.SharedModels;

public class WeatherReport
{
    public required string City { get; set; }
    public required string Condition { get; set; }
    public required int DegreesC { get; set; }
    public required int DegreesF { get; set; }

    public static WeatherReport FromJson(string json)
    {
        return JsonSerializer.Deserialize<WeatherReport>(json, JsonSerializerOptions.Web)!;
    }
}