using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

#pragma warning disable OPENAI001

namespace Playground.Tests;

public class ReasoningSummary
{
    public static async Task Run(Configuration configuration)
    {
        //OpenAIClient client = new(configuration.OpenAiApiKey);
        AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

        ChatClientAgent agent = client
            .GetOpenAIResponseClient("gpt-5-mini")
            .CreateAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    RawRepresentationFactory = _ => new ResponseCreationOptions()
                    {
                        ReasoningOptions = new ResponseReasoningOptions
                        {
                            ReasoningEffortLevel = ResponseReasoningEffortLevel.Medium,
                            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed
                        }
                    }
                }
            });

        AgentRunResponse response = await agent.RunAsync("What is the capital of france and how many live there?");

        foreach (ChatMessage message in response.Messages)
        {
            foreach (AIContent content in message.Contents)
            {
                if (content is TextReasoningContent textReasoningContent)
                {
                    Utils.WriteLineGreen("The Reasoning");
                    Utils.WriteLineDarkGray(textReasoningContent.Text);
                }
            }
        }

        Utils.WriteLineGreen("The Answer");
        Console.WriteLine(response);
    }
}