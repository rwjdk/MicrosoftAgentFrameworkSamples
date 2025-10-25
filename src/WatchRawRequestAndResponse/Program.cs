//YouTube video that cover this sample: https://youtu.be/Gr3S1Q9eZrc

using Azure.AI.OpenAI;
using OpenAI;
using Shared;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using Microsoft.Agents.AI;

Console.Clear();
using var handler = new CustomClientHttpHandler();
using var httpClient = new HttpClient(handler);

Configuration configuration = ConfigurationManager.GetConfiguration();

/*
OpenAIClient client = new(new ApiKeyCredential(configuration.OpenAiApiKey), new OpenAIClientOptions
{
    Transport = new HttpClientPipelineTransport(httpClient)
});
*/

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey), new AzureOpenAIClientOptions
{
    Transport = new HttpClientPipelineTransport(httpClient)
});


ChatClientAgent agent = client.GetChatClient(configuration.ChatDeploymentName).CreateAIAgent(
    instructions: "You are a Raw Agent"
);

AgentRunResponse response = await agent.RunAsync("Hello");
Utils.WriteLineGreen("The Answer");
Console.WriteLine(response);

class CustomClientHttpHandler() : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
        Utils.WriteLineGreen($"Raw Request ({request.RequestUri})");
        Utils.WriteLineDarkGray(MakePretty(requestString));
        Utils.Separator();
        var response = await base.SendAsync(request, cancellationToken);

        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        Utils.WriteLineGreen("Raw Response");
        Utils.WriteLineDarkGray(MakePretty(responseString));
        Utils.Separator();
        return response;
    }

    private string MakePretty(string input)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
        return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
    }
}