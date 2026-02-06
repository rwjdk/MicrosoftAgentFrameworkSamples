using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Anthropic;
using Anthropic;
using Anthropic.Core;
using Anthropic.Models.Messages;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Shared;
using Shared.Extensions;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

#pragma warning disable OPENAI001
Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.AnthropicApiKey;
string question = "What is the Capital of country where the eiffel tower sits " +
                  "and how is it to live there and how many people live there? " +
                  "(Answer back in max 3 words)";

// All modern Anthropic models can do thinking

Console.Clear();
Utils.WriteLineGreen("Baseline (No thinking)");
await Baseline();
Utils.Separator();
Utils.WriteLineGreen("Raw (BudgetTokens = 2000)");
await Raw();
Utils.Separator();
Utils.WriteLineGreen("Agent Framework Toolkit (BudgetTokens = 2000)");
await AgentFrameworkToolkit();

async Task Baseline()
{
    AnthropicClient client = new(new ClientOptions
    {
        ApiKey = apiKey
    });
    ChatClientAgent agent = new ChatClientAgent(client.AsIChatClient("claude-haiku-4-5-20251001"),
        new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                MaxOutputTokens = 10000
            }
        });
    AgentResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    response.Usage.OutputAsInformation();
}

async Task Raw()
{
    AnthropicClient client = new(new ClientOptions
    {
        ApiKey = apiKey
    });
    var modelId = "claude-haiku-4-5-20251001";
    ChatClientAgent agent = new ChatClientAgent(client.AsIChatClient(modelId),
        new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                RawRepresentationFactory = _ => new MessageCreateParams
                {
                    MaxTokens = 10000,
                    Messages = [],
                    Model = modelId,
                    Thinking = new ThinkingConfigParam(new ThinkingConfigEnabled()
                    {
                        BudgetTokens = 2000 //Budget need to a minimum of 1024
                    }),
                }
            }
        });

    AgentResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    foreach (ChatMessage message in response.Messages)
    {
        foreach (AIContent content in message.Contents)
        {
            if (content is TextReasoningContent textReasoningContent)
            {
                Utils.WriteLineYellow("Reasoning Text");
                Utils.WriteLineDarkGray(textReasoningContent.Text);
            }
        }
    }
    response.Usage.OutputAsInformation(); //Anthropic do not report reasoning tokens back separately
}

async Task AgentFrameworkToolkit()
{
    AnthropicAgentFactory agentFactory = new(apiKey);

    AnthropicAgent agent = agentFactory.CreateAgent(new AnthropicAgentOptions
    {
        Model = AnthropicChatModels.ClaudeHaiku45,
        MaxOutputTokens = 10000,
        BudgetTokens = 2000 //Budget need to a minimum of 1024
    });

    AgentResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    TextReasoningContent? reasoningContent = response.GetTextReasoningContent();
    if (reasoningContent != null)
    {
        Utils.WriteLineYellow("Reasoning Text");
        Utils.WriteLineDarkGray(reasoningContent.Text);
    }

    response.Usage.OutputAsInformation(); //Anthropic do not report reasoning tokens back separately

}