using Shared;
using System.Text.Json;

namespace ToolsVsMcpVsSkillsVsAIContextProviders;

public class CustomClientHttpHandler() : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
        Utils.Green($"Raw Request ({request.RequestUri})");
        Utils.Gray(MakePretty(requestString));
        Utils.Separator();
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        Utils.Green("Raw Response");
        Utils.Gray(MakePretty(responseString));
        Utils.Separator();
        return response;
    }

    private string MakePretty(string input)
    {
        JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
        return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
    }
}