using System.Text.Json;

namespace AgentFramework.Utilities;

public class AgentRawCallDetailsHttpHandler(Action<AgentRawCallDetails> rawCallDetails) : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        rawCallDetails.Invoke(new AgentRawCallDetails
        {
            RequestUrl = request.RequestUri!.AbsoluteUri,
            RequestJson = MakePretty(requestString),
            ResponseJson = MakePretty(responseString)
        });
        return response;

        static string MakePretty(string input)
        {
            JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
            return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}