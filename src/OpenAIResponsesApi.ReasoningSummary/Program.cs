//YouTube video that cover this sample: https://youtu.be/4D02zSl4QAQ

using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using Shared;

#pragma warning disable OPENAI001
Configuration configuration = ConfigurationManager.GetConfiguration();

//OpenAIClient client = new(configuration.OpenAiApiKey);
AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClientAgent agent = client
    .GetOpenAIResponseClient("gpt-5-mini")
    .CreateAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            RawRepresentationFactory = _ => new ResponseCreationOptions() //<--- Notice this is different from out ChatCompletionOptions
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