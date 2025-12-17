using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;

namespace AgentFrameworkToolkit.EasierAgents;

public class After
{
    public static async Task RunAsync()
    {
        //Weather-Task: With GPT-5-Mini in low Reasoning, Call Tool with middleware and return as structured Output

        Secrets secrets = SecretManager.GetSecrets();

        AzureOpenAIAgentFactory agentFactory = new(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);

        AzureOpenAIAgent agent = agentFactory.CreateAgent(new AgentOptions
        {
            Model = OpenAIChatModels.Gpt5Mini,
            ReasoningEffort = OpenAIReasoningEffort.Low,
            Tools = [AIFunctionFactory.Create(WeatherTool.GetWeather)],
            RawToolCallDetails = details => { Utils.WriteLineDarkGray(details.ToString()); }
        });

        ChatClientAgentRunResponse<WeatherReport> response = await agent.RunAsync<WeatherReport>("What is the Weather like in Paris");
        WeatherReport weatherReport = response.Result;
        Console.WriteLine("City: " + weatherReport.City);
        Console.WriteLine("Condition: " + weatherReport.Condition);
        Console.WriteLine("Degrees: " + weatherReport.Degrees);
        Console.WriteLine("Fahrenheit: " + weatherReport.Fahrenheit);
        response.Usage.OutputAsInformation();
    }
}