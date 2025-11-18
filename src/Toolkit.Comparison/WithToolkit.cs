using AgentFramework.Utilities;
using AgentFramework.Utilities.AnthropicSDK;
using AgentFramework.Utilities.AzureOpenAI;
using AgentFramework.Utilities.GoogleGenerativeAI;
using AgentFramework.Utilities.Grok;
using AgentFramework.Utilities.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using Shared;
using Shared.Extensions;

#pragma warning disable OPENAI001

namespace Toolkit.Comparison;

public class WithToolkit
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();

        bool addTool = false;

        Agent[] agents =
        [
            GetGrokAgent(),
            GetAnthropicAgent(),
            GetGoogleAgent(),
            GetAzureOpenAIAgent(),
            GetOpenAIAgent()
        ];

        foreach (Agent agent in agents)
        {
            try
            {
                //Normal
                Console.WriteLine(agent.Provider);
                AgentRunResponse response1 = await agent.RunAsync("What is the capital of France?");
                Console.WriteLine(response1);
                response1.Usage.OutputAsInformation();
                /*
                //Streaming
                await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("Hello Again"))
                {
                    Console.Write(update);
                }

                Console.WriteLine();

                //Normal Tool Call
                AgentRunResponse response2 = await agent.RunAsync("What is the Weather like in Paris?");
                Console.WriteLine(response2);

                //Tool Call Streaming
                await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("What is the Weather like in Paris?"))
                {
                    Console.Write(update);
                }

                Console.WriteLine();

                //Structured output
                ChatClientAgentRunResponse<Weather> response3 = await agent.RunAsync<Weather>("What is the Weather like in Paris?");
                Console.WriteLine(response3.Result.City);*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        /*


        AgentRunResponse response = await agent.RunAsync("What is the weather like in paris?");
        */

        AzureOpenAIAgentFactory azureOpenAIAgentFactory = new(new AzureOpenAIConnection
        {
            Endpoint = configuration.AzureOpenAiEndpoint,
            ApiKey = configuration.AzureOpenAiKey
        });

        Agent commonAgent = azureOpenAIAgentFactory.CreateAgent(new OpenAIResponseWithReasoningOptions
        {
            DeploymentModelName = "gpt-5-nano",
            MaxOutputTokens = 200,
            ReasoningEffort = ResponseReasoningEffortLevel.Low,
            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
            Tools = [AIFunctionFactory.Create(GetWeather)],
            AdditionalChatClientAgentOptions = options => { options.Name = "NO!"; }
        });


        AgentRunResponse agentRunResponse = await commonAgent.RunAsync("What is the weather like in Paris?");

        UsageDetails usageDetails = agentRunResponse.Usage!;
        long? a = usageDetails.InputTokenCount;
        long? b = usageDetails.OutputTokenCount;


        ChatClientAgentRunResponse<Weather> commonResponse = await commonAgent.RunAsync<Weather>("What is the weather like in Paris?");
        Weather commonWeather = commonResponse.Result;

        Agent fullBlownAgent = azureOpenAIAgentFactory.CreateAgent(new OpenAIResponseWithReasoningOptions
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

        Agent GetAzureOpenAIAgent()
        {
            AzureOpenAIAgentFactory factory = new(new AzureOpenAIConnection
            {
                Endpoint = configuration.AzureOpenAiEndpoint,
                ApiKey = configuration.AzureOpenAiKey,
            });

            Agent agent = factory.CreateAgent(new OpenAIResponseWithoutReasoningOptions()
            {
                DeploymentModelName = "gpt-4.1-mini",
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : []
            });
            return agent;
        }

        Agent GetOpenAIAgent()
        {
            OpenAIAgentFactory factory = new(new OpenAIConnection
            {
                ApiKey = configuration.OpenAiApiKey
            });

            Agent agent = factory.CreateAgent(new OpenAIResponseWithoutReasoningOptions()
            {
                DeploymentModelName = "gpt-4.1-mini",
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : []
            });
            return agent;
        }

        Agent GetGrokAgent()
        {
            GrokAgentFactory factory = new(new GrokConnection
            {
                ApiKey = configuration.XAiGrokApiKey
            });

            Agent agent = factory.CreateAgent(new OpenAIResponseWithoutReasoningOptions()
            {
                DeploymentModelName = "grok-4-fast-non-reasoning",
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : []
            });
            return agent;
        }

        Agent GetAnthropicAgent()
        {
            AnthropicSDKAgentFactory factory = new(new AnthropicSDKConnection
            {
                ApiKey = configuration.AnthropicApiKey
            });

            Agent agent = factory.CreateAgent(new AnthropicSDKOptions
            {
                DeploymentModelName = "claude-sonnet-4-5-20250929",
                MaxOutputTokens = 1000,
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : []
            });
            return agent;
        }

        Agent GetGoogleAgent()
        {
            GoogleGenerativeAIAgentFactory factory = new(new GoogleGenerativeAIConnection
            {
                ApiKey = configuration.GoogleGeminiApiKey
            });

            Agent agent = factory.CreateAgent(new GoogleGenerativeAIOptions
            {
                DeploymentModelName = GenerativeAI.GoogleAIModels.Gemini25Pro,
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : [],
            });
            return agent;
        }
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