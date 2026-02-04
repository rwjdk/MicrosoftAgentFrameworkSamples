using System.Text.Json.Serialization;

namespace AIAsDataFilter.Models;

public class Book
{
    public required string Title { get; set; }
    public required int YearOfRelease { get; set; }
    public required string Author { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Genre Genre { get; set; }

    public required string Synopsis { get; set; }

    public override string ToString()
    {
        return $"""<book title="{Title}" year="{YearOfRelease}" author="{Author}" genre="{Genre}" Synopsis="{Synopsis}"/>""";
    }
}