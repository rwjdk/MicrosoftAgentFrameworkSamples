using AgentFramework.Toolkit.AIAgents;
using AgentFramework.Toolkit.AIAgents.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using Shared;

#pragma warning disable OPENAI001

namespace Toolkit.Comparison;

public class WithToolkit
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        AgentFactoryAzureOpenAI agentFactory = new(new AzureOpenAIAgentFactoryConfiguration
        {
            Endpoint = configuration.AzureOpenAiEndpoint,
            ApiKey = configuration.AzureOpenAiKey
        });


        Agent commonAgent = agentFactory.CreateAgent(new ResponsesApiReasoning
        {
            DeploymentModelName = "gpt-5-mini",
            ReasoningEffort = ResponseReasoningEffortLevel.Low,
            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
            Tools = [AIFunctionFactory.Create(GetWeather)],
        });

        ChatClientAgentRunResponse<Weather> commonResponse = await commonAgent.RunAsync<Weather>("What is the weather like in Paris?");
        Weather commonWeather = commonResponse.Result;

        Agent fullBlownAgent = agentFactory.CreateAgent(new ResponsesApiReasoning
        {
            Id = "1234",
            Name = "MyAgent",
            Description = "The description of my agent",
            Instructions = "Speak like a pirate",
            DeploymentModelName = "gpt-5-mini",
            ReasoningEffort = ResponseReasoningEffortLevel.Low,
            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
            NetworkTimeout = TimeSpan.FromMinutes(5),
            Tools = [AIFunctionFactory.Create(GetWeather)],
            RawToolCallDetails = details => { Console.WriteLine(details.ToString()); },
            RawHttpCallDetails = details =>
            {
                Console.WriteLine($"URL: {details.RequestUrl}");
                Console.WriteLine($"Request: {details.RequestJson}");
                Console.WriteLine($"Response: {details.ResponseJson}");
            }
        });

        ChatClientAgentRunResponse<Weather> fullBlownResponse = await fullBlownAgent.RunAsync<Weather>("What is the weather like in Paris?");
        Weather fullBlownResponseWeather = fullBlownResponse.Result;
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