using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;
using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Google;
using Google.GenAI.Types;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

#pragma warning disable OPENAI001
Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.GoogleGeminiApiKey;
string question = "What is the Capital of France and how many people live there? (Answer back in max 3 words)";

// All modern models (2.5 and higher) can do thinking

Console.Clear();
Utils.WriteLineGreen("Baseline: Gemini 2.5 (Auto Thinking)");
await Baseline25();
Utils.Separator();
Utils.WriteLineGreen("Baseline: Gemini 3 (Auto Thinking)");
await Baseline3();
Utils.Separator();
Utils.WriteLineGreen("Raw: Gemini 2.5 (Thinking Budget = 2000)");
await Raw25();
Utils.Separator();
Utils.WriteLineGreen("Raw: Gemini 3 (Thinking Level = High)");
await Raw3();
Utils.Separator();
Utils.WriteLineGreen("Agent Framework Toolkit: Gemini 2.5 (Thinking Budget = 2000)");
await AgentFrameworkToolkit25();
Utils.Separator();
Utils.WriteLineGreen("Agent Framework Toolkit: Gemini 3 (Thinking Level = High)");
await AgentFrameworkToolkit3();

async Task Baseline25()
{
    Google.GenAI.Client client = new(apiKey: apiKey);
    ChatClientAgent agent = new ChatClientAgent(client.AsIChatClient("gemini-2.5-flash"));
    AgentRunResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    response.Usage.OutputAsInformation();
}

async Task Baseline3()
{
    Google.GenAI.Client client = new(apiKey: apiKey);
    ChatClientAgent agent = new ChatClientAgent(client.AsIChatClient("gemini-3-flash-preview"));
    AgentRunResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    response.Usage.OutputAsInformation();
}

async Task Raw25()
{
    Google.GenAI.Client client = new(apiKey: apiKey);
    ChatClientAgent agent = new(client.AsIChatClient("gemini-2.5-flash"), new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            RawRepresentationFactory = _ => new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 2000, //Max number of tokens to use (-1 = Auto, 0 = Off)
                    //ThinkingLevel not supported (Setting Both >> Exception)
                    IncludeThoughts = true
                }
            }
        }
    });

    AgentRunResponse response = await agent.RunAsync(question);
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
    response.Usage.OutputAsInformation();
    
}

async Task Raw3()
{
    Google.GenAI.Client client = new(apiKey: apiKey);
    ChatClientAgent agent = new(client.AsIChatClient("gemini-3-flash-preview"), new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            RawRepresentationFactory = _ => new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    //ThinkingBudget can be set for backward compatibility but not recommended (Setting Both >> Exception)
                    ThinkingLevel = ThinkingLevel.HIGH, //Pro can set HIGH/LOW - Flash can set HIGH/MEDIUM/LOW/MINIMAL
                    IncludeThoughts = true
                }
            }
        }
    });

    AgentRunResponse response = await agent.RunAsync(question);
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
    response.Usage.OutputAsInformation();
}

async Task AgentFrameworkToolkit25()
{
    GoogleAgentFactory agentFactory = new(apiKey);

    GoogleAgent agent = agentFactory.CreateAgent(new GoogleAgentOptions
    {
        Model = GoogleChatModels.Gemini25Flash,
        ThinkingBudget = 2000 //Max number of tokens to use (-1 = Auto, 0 = Off)
        //ThinkingLevel not supported (Setting Both >> Exception)
    });

    AgentRunResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    TextReasoningContent? reasoningContent = response.GetTextReasoningContent();
    if (reasoningContent != null)
    {
        Utils.WriteLineYellow("Reasoning Text");
        Utils.WriteLineDarkGray(reasoningContent.Text);
    }
    response.Usage.OutputAsInformation();
}

async Task AgentFrameworkToolkit3()
{
    GoogleAgentFactory agentFactory = new(apiKey);

    GoogleAgent agent = agentFactory.CreateAgent(new GoogleAgentOptions
    {
        Model = "gemini-3-flash-preview",
        //ThinkingBudget can be set for backward compatibility but not recommended (Setting Both >> Exception)
        ThinkingLevel = ThinkingLevel.HIGH //Pro can set HIGH/LOW - Flash can set HIGH/MEDIUM/LOW/MINIMAL
    });

    AgentRunResponse response = await agent.RunAsync(question);
    Console.WriteLine(response);
    TextReasoningContent? reasoningContent = response.GetTextReasoningContent();
    if (reasoningContent != null)
    {
        Utils.WriteLineYellow("Reasoning Text");
        Utils.WriteLineDarkGray(reasoningContent.Text);
    }

    response.Usage.OutputAsInformation();
    
}