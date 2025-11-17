using AgentFramework.Toolkit.Agents;
using AgentFramework.Toolkit.AnthropicSDK;
using AgentFramework.Toolkit.AnthropicSDK.Agents;
using AgentFramework.Toolkit.AnthropicSDK.Agents.Models;
using AgentFramework.Toolkit.AzureOpenAI;
using AgentFramework.Toolkit.AzureOpenAI.Agents;
using AgentFramework.Toolkit.OpenAI.Agents.Models;
using AgentFramework.Toolkit.OpenAI.Usage;
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

        AgentFactoryAnthropicSDK aa = new(new AnthropicSDKConnection
        {
            ApiKey = configuration.AnthropicApiKey
        });

        Agent aaAgent = aa.CreateAgent(new AnthropicSDKOptions
        {
            DeploymentModelName = "claude-sonnet-4-5-20250929",
            MaxOutputTokens = 1000,
            Tools =
            [
                AIFunctionFactory.Create(GetWeather)
            ],
        });

        AgentRunResponse aaRun = await aaAgent.RunAsync("What is the weather like in Paris?");


        /*
        AgentFactoryGoogleGenerativeAI googleFactory = new(new GoogleGenerativeAIConfiguration
        {
            ApiKey = configuration.GoogleGeminiApiKey
        });

        Agent agent = googleFactory.CreateAgent(new GoogleGenerativeAIOptions
        {
            Temperature = 0,
            MaxOutputTokens = 100,
            Tools =
            [
                AIFunctionFactory.Create(GetWeather)
            ],
            DeploymentModelName = GenerativeAI.GoogleAIModels.Gemini25Flash
        });

        AgentRunResponse response = await agent.RunAsync("What is the weather like in paris?");
        */

        AgentFactoryAzureOpenAI agentFactoryAzureOpenAI = new(new AzureOpenAIConnection
        {
            Endpoint = configuration.AzureOpenAiEndpoint,
            ApiKey = configuration.AzureOpenAiKey
        });

        Agent commonAgent = agentFactoryAzureOpenAI.CreateAgent(new OpenAIResponseWithReasoningOptions()
        {
            DeploymentModelName = "gpt-5-nano",
            MaxOutputTokens = 200,
            ReasoningEffort = ResponseReasoningEffortLevel.Low,
            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
            Tools = [AIFunctionFactory.Create(GetWeather)],
            AdditionalChatClientAgentOptions = options =>
            {
                options.Name = "NO!";
            }
        });


        AgentRunResponse agentRunResponse = await commonAgent.RunAsync("What is the weather like in Paris?");

        UsageDetails usageDetails = agentRunResponse.Usage!;
        long? a = usageDetails.InputTokenCount;
        long? b = usageDetails.OutputTokenCount;
        long d = usageDetails.OutputReasoningTokenCount;
        long c = usageDetails.InputCachedTokenCount;
        

        ChatClientAgentRunResponse<Weather> commonResponse = await commonAgent.RunAsync<Weather>("What is the weather like in Paris?");
        Weather commonWeather = commonResponse.Result;

        Agent fullBlownAgent = agentFactoryAzureOpenAI.CreateAgent(new OpenAIResponseWithReasoningOptions
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