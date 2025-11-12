using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using Shared;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable OPENAI001

namespace Toolkit.Comparison;

public class WithoutToolkit
{
    public static async Task Run()
    {
        using var handler = new CustomClientHttpHandler();
        using var httpClient = new HttpClient(handler);
        Configuration configuration = ConfigurationManager.GetConfiguration();

        AzureOpenAIClient client = new(
            new Uri(configuration.AzureOpenAiEndpoint),
            new ApiKeyCredential(configuration.AzureOpenAiKey),
            new AzureOpenAIClientOptions
            {
                NetworkTimeout = TimeSpan.FromMinutes(5),
                Transport = new HttpClientPipelineTransport(httpClient)
            });

        ChatClientAgent commonAgent = client
            .GetOpenAIResponseClient("gpt-5-mini")
            .CreateAIAgent(
                new ChatClientAgentOptions
                {
                    ChatOptions = new ChatOptions
                    {
                        RawRepresentationFactory = _ => new ResponseCreationOptions()
                        {
                            ReasoningOptions = new ResponseReasoningOptions
                            {
                                ReasoningEffortLevel = ResponseReasoningEffortLevel.Low,
                                ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed
                            }
                        },
                        Tools = [AIFunctionFactory.Create(GetWeather)]
                    }
                });

        ChatClientAgentRunResponse<Weather> commonResponse = await commonAgent.RunAsync<Weather>("What is the weather like in Paris?");
        Weather commonWeather = commonResponse.Result;

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new JsonStringEnumConverter() }
        };

        AIAgent fullBlownAgent = client
            .GetOpenAIResponseClient("gpt-5-mini")
            .CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Id = "1234",
                    Name = "MyAgent",
                    Description = "The description of my agent",
                    Instructions = "Speak like a pirate",
                    ChatOptions = new ChatOptions
                    {
                        RawRepresentationFactory = _ => new ResponseCreationOptions()
                        {
                            ReasoningOptions = new ResponseReasoningOptions
                            {
                                ReasoningEffortLevel = ResponseReasoningEffortLevel.Low,
                                ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed
                            }
                        },
                        Tools = [AIFunctionFactory.Create(GetWeather)]
                    }
                })
            .AsBuilder()
            .Use(FunctionCallMiddleware)
            .Build();

        ChatResponseFormatJson chatResponseFormatJson = ChatResponseFormat.ForJsonSchema<Weather>(jsonSerializerOptions);
        var fullBlownResponse = await fullBlownAgent.RunAsync("What is the weather like in Paris?", options: new ChatClientAgentRunOptions()
        {
            ChatOptions = new ChatOptions
            {
                ResponseFormat = chatResponseFormatJson
            }
        });
        Weather fullBlownResponseWeather = fullBlownResponse.Deserialize<Weather>(jsonSerializerOptions);
    }

    static async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        StringBuilder functionCallDetails = new();
        functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
        if (context.Arguments.Count > 0)
        {
            functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
        }

        Utils.WriteLineDarkGray(functionCallDetails.ToString());

        return await next(context, cancellationToken);
    }

    public static string GetWeather(string city)
    {
        return "It is sunny";
    }

    public class Weather
    {
        public required string City { get; set; }
        public required int DegreesCelsius { get; set; }
        public required int DegreesFahrenheit { get; set; }
    }
}

class CustomClientHttpHandler : HttpClientHandler
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