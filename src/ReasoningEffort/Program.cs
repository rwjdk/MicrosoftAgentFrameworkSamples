using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using OpenAI.Responses;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

#pragma warning disable OPENAI001
Secrets secrets = SecretManager.GetSecrets();
string endpoint = secrets.AzureOpenAiEndpoint;
string apiKey = secrets.AzureOpenAiKey;
string question = "What is the Capital of France and how many people live there? (Answer back in max 3 words)";

/* OpenAI Models that can use reasoning:
 - o1 models
 - o3 models
 - o4 models
 - gpt-5 models
 (expect all future model can do reasoning)
 */

Console.Clear();
Utils.Green("Baseline (Reason = Default (Medium))");
await Baseline();
Utils.Separator();
Utils.Green("Raw: ChatClient (Reason = Minimal)");
await RawChatClient();
Utils.Separator();
Utils.Green("Raw: ResponseAPI (Reason = High)");
await RawResponsesApi();
Utils.Separator();
Utils.Green("Agent Framework Toolkit: ChatClient (Reason = Minimal)");
await AgentFrameworkToolkitChatClient();
Utils.Separator();
Utils.Green("Agent Framework Toolkit: ResponsesAPI (Reason = High)");
await AgentFrameworkToolkitResponseApi();

/* Notes on various OpenAI-Based Models:
   - xAI (Grok) seems unable to use this (Throw an exception if set).
   - DeepSeek (via example OpenRouter) allow this to be set, but it seems to ignore it. 
 */

return;

async Task Baseline()
{
    AzureOpenAIClient azureOpenAiClient = new(new Uri(endpoint), new ApiKeyCredential(apiKey));
    ChatClientAgent agent = azureOpenAiClient
        .GetChatClient("gpt-5-mini")
        .AsAIAgent();
    AgentResponse response = await agent.RunAsync(question);
    response.Usage.OutputAsInformation();
    Console.WriteLine(response);
}

async Task RawChatClient()
{
    AzureOpenAIClient azureOpenAiClient = new(new Uri(endpoint), new ApiKeyCredential(apiKey));
    ChatClientAgent agent = azureOpenAiClient
        .GetChatClient("gpt-5-mini")
        .AsAIAgent(
            options: new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    RawRepresentationFactory = _ => new ChatCompletionOptions
                    {
                        ReasoningEffortLevel = ChatReasoningEffortLevel.Minimal
                    },
                }
            });

    AgentResponse response = await agent.RunAsync(question);
    //Note that the reasoning summary is not possible to get with ChatClient
    Console.WriteLine(response);
    response.Usage.OutputAsInformation();
}

async Task RawResponsesApi()
{
    AzureOpenAIClient azureOpenAiClient = new(new Uri(endpoint), new ApiKeyCredential(apiKey));
    ChatClientAgent agent = azureOpenAiClient
        .GetResponsesClient("gpt-5-mini")
        .AsAIAgent(
            options: new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    RawRepresentationFactory = _ => new CreateResponseOptions
                    {
                        ReasoningOptions = new ResponseReasoningOptions
                        {
                            ReasoningEffortLevel = ResponseReasoningEffortLevel.High,
                            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed
                        }
                    }
                }
            });

    AgentResponse response = await agent.RunAsync(question);
    foreach (ChatMessage message in response.Messages)
    {
        foreach (AIContent content in message.Contents)
        {
            if (content is TextReasoningContent textReasoningContent)
            {
                Utils.Yellow("Reasoning Text");
                Utils.Gray(textReasoningContent.Text);
            }
        }
    }

    Console.WriteLine(response);
    response.Usage.OutputAsInformation();
}

async Task AgentFrameworkToolkitChatClient()
{
    AzureOpenAIAgentFactory agentFactory = new(endpoint, apiKey);

    AzureOpenAIAgent agent = agentFactory.CreateAgent(new AgentOptions
    {
        Model = OpenAIChatModels.Gpt5Mini,
        ReasoningEffort = OpenAIReasoningEffort.Minimal
    });

    AgentResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    response.Usage.OutputAsInformation();
}

async Task AgentFrameworkToolkitResponseApi()
{
    AzureOpenAIAgentFactory agentFactory = new(endpoint, apiKey);

    AzureOpenAIAgent agent = agentFactory.CreateAgent(new AgentOptions
    {
        ClientType = ClientType.ResponsesApi,
        Model = OpenAIChatModels.Gpt5Mini,
        ReasoningEffort = OpenAIReasoningEffort.High,
        ReasoningSummaryVerbosity = OpenAIReasoningSummaryVerbosity.Detailed
    });

    AgentResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    TextReasoningContent? reasoningContent = response.GetTextReasoningContent();
    if (reasoningContent != null)
    {
        Utils.Yellow("Reasoning Text");
        Utils.Gray(reasoningContent.Text);
    }

    response.Usage.OutputAsInformation();
}