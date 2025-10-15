using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Utilities.Extensions;
using OpenAI;
using OpenAI.Chat;
using Shared;
using Shared.Extensions;
using System.ClientModel;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient azureOpenAiClient = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey), new AzureOpenAIClientOptions
{
    NetworkTimeout = TimeSpan.FromMinutes(5) //NB: You might need to adjust timeout when using long thinking Agents
});

//Let's try and use reasoning model with default (medium) settings
ChatClientAgent agentDefault = azureOpenAiClient
    .GetChatClient("gpt-5-mini")
    .CreateAIAgent();

AgentRunResponse response1 = await agentDefault.RunAsync("What is the Capital of France and how many people live there?");
Console.WriteLine(response1);
response1.Usage.OutputAsInformation();


Utils.Separator();

//Let's control the reasoning effort for faster answer and lower cost
ChatClientAgent agentControllingReasoningEffort = azureOpenAiClient
    .GetChatClient("gpt-5-mini")
    .CreateAIAgent(
        options: new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                RawRepresentationFactory = _ => new ChatCompletionOptions
                {
#pragma warning disable OPENAI001
                    ReasoningEffortLevel = "minimal", //'minimal', 'low', 'medium' (default) or 'high'
#pragma warning restore OPENAI001
                },
            }
        });

AgentRunResponse response2 = await agentControllingReasoningEffort.RunAsync("What is the Capital of France and how many people live there?");
Console.WriteLine(response2);
response2.Usage.OutputAsInformation();

Utils.Separator();

//Simpler version of above using my own extension method
ChatClientAgent agentControllingReasoningEffortSimplified = azureOpenAiClient
    .GetChatClient("gpt-5-mini")
    .CreateAIAgentForAzureOpenAi(reasoningEffort: "minimal");

AgentRunResponse response3 = await agentControllingReasoningEffortSimplified.RunAsync("What is the Capital of France and how many people live there?");
Console.WriteLine(response3);
response3.Usage.OutputAsInformation();