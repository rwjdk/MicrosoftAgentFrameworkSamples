using AgentFramework.Toolkit.AIAgents;
using Microsoft.Agents.AI;
using Shared;
using System.Text;
using AgentFramework.Toolkit.AIAgents.Models;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using Shared.Extensions;

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

        AgentFramework.Toolkit.AIAgents.Models.
            /*
            Agent reasonAgent = agentFactory.Create(new ResponsesApiReasoningOptions
            {
                DeploymentModelName = "gpt-5-mini",
                ReasoningEffort = ResponseReasoningEffortLevel.Low,
                ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
            });

            AgentRunResponse response0 = await reasonAgent.RunAsync("How to make soup?");
            Console.WriteLine(response0);
            response0.Usage.OutputAsInformation();*/
            Agent agent = agentFactory.CreateAgent(new ResponsesApiNonReasoning()
            {
                DeploymentModelName = "gpt-4.1-mini",
                Temperature = 0,
                RawHttpCallDetails = details =>
                {
                    Utils.WriteLineGreen(details.RequestUrl);
                    Utils.WriteLineDarkGray(details.RequestJson);
                    Utils.Separator();
                    Utils.WriteLineDarkGray(details.ResponseJson);
                },

                Tools = [AIFunctionFactory.Create(GetWeather)],
                /*
                RawToolCallDetails = details =>
                {
                    StringBuilder functionCallDetails = new();
                    functionCallDetails.Append($"- Tool Call: '{details.Context.Function.Name}'");
                    if (details.Context.Arguments.Count > 0)
                    {
                        functionCallDetails.Append($" (Args: {string.Join(",", details.Context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
                    }

                    Utils.WriteLineDarkGray(functionCallDetails.ToString());
                }*/
            });

        ChatClientAgentRunResponse<Weather> response = await agent.RunAsync<Weather>("How is the weather in Aarhus?");
        Console.WriteLine(response);
    }

    public static string GetWeather(string city)
    {
        return "It is sunny";
    }

    public class Weather
    {
        public required string City { get; set; }
        public required string Condition { get; set; }
    }
}