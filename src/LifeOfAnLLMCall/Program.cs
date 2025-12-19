//YouTube video that cover this sample: https://youtu.be/Gr3S1Q9eZrc

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.ComponentModel;
using System.Text.Json;
using LifeOfAnLLMCall;
using OpenAI.Chat;

Console.Clear();
using CustomClientHttpHandler handler = new CustomClientHttpHandler();
using HttpClient httpClient = new HttpClient(handler);

Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey), new AzureOpenAIClientOptions
{
    Transport = new HttpClientPipelineTransport(httpClient)
});


ChatClientAgent agent = client
    .GetChatClient("gpt-5")
    .CreateAIAgent(
        instructions: "Speak like a pirate!",
        tools: [AIFunctionFactory.Create(Tools.GetWeather)]
    );

ChatClientAgentRunResponse<WeatherResponse> response = await agent.RunAsync<WeatherResponse>("What is the Weather like in Paris?");
WeatherResponse result = response.Result;
Console.WriteLine(response);

class WeatherResponse
{
    [Description("City (and country in parentheses)")]
    public required string City { get; set; }

    public required string Condition { get; set; }
    public required int DegreesFahrenheit { get; set; }
    public required int DegreesCelsius { get; set; }
}

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