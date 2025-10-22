//YouTube video that cover this sample: https://youtu.be/ghND74Hj6Fs

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using MicrosoftAgentFramework.Utilities.Extensions;
using OpenAI;
using Shared;
using System.ClientModel;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClientAgent agent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(instructions: "You are a Friendly AI Bot, answering questions");

string question = "What is the capital of France and how many people live there?";

//Simple
AgentRunResponse response = await agent.RunAsync(question);
Console.WriteLine(response);

Utils.WriteLineDarkGray($"- Input Tokens: {response.Usage?.InputTokenCount}");
Utils.WriteLineDarkGray($"- Output Tokens: {response.Usage?.OutputTokenCount} " +
                        $"({response.Usage?.GetOutputTokensUsedForReasoning()} was used for reasoning)");

//------------------------------------------------------------------------------------------------------------------------
Utils.Separator();

//Streaming
List<AgentRunResponseUpdate> updates = [];
await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(question))
{
    updates.Add(update);
    Console.Write(update);
}

Console.WriteLine();

AgentRunResponse collectedResponseFromStreaming = updates.ToAgentRunResponse();
Utils.WriteLineDarkGray($"- Input Tokens (Streaming): {collectedResponseFromStreaming.Usage?.InputTokenCount}");
Utils.WriteLineDarkGray($"- Output Tokens (Streaming): {collectedResponseFromStreaming.Usage?.OutputTokenCount} " +
                        $"({collectedResponseFromStreaming.Usage?.GetOutputTokensUsedForReasoning()} was used for reasoning)");

Utils.Separator();
Console.ReadKey();