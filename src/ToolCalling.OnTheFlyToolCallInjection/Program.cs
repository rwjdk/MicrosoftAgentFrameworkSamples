using AgentFrameworkToolkit.Tools.Common;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using System.Text;
using JetBrains.Annotations;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();

Console.Write("Normal (N) or Inject Mode (I)?: ");
var key = Console.ReadKey();
Console.Clear();
switch (key.Key)
{
    case ConsoleKey.N:
        await NormalAgentWithTools();
        break;
    case ConsoleKey.I:
        await ToolInjection();
        break;
    default:
        Console.WriteLine("Invalid choice");
        break;
}

async Task NormalAgentWithTools()
{
    List<AITool> tools = [];
    tools.AddRange(TimeTools.All());
    tools.AddRange(FileSystemTools.All(new FileSystemToolsOptions
    {
        ConfinedToTheseFolderPaths = ["C:\\TestAI"]
    }));
    tools.AddRange(WeatherTools.All(new OpenWeatherMapOptions
    {
        ApiKey = secrets.OpenWeatherApiKey,
        PreferredUnits = WeatherOptionsUnits.Metric
    }));

    AzureOpenAIClient client = new AzureOpenAIClient(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
    AIAgent mainAgent = client.GetChatClient("gpt-4.1").AsAIAgent(tools: tools).AsBuilder().Use(FunctionCallMiddleware).Build();

    Utils.WriteLineDarkGray($"This agent have: {tools.Count} tools");
    foreach (var tool in tools)
    {
        Utils.WriteLineDarkGray($"- {tool.Name}");
    }
    while (true)
    {
        Console.Write("> ");
        string input = Console.ReadLine() ?? "";
        AgentResponse response = await mainAgent.RunAsync(input);
        Console.WriteLine(response);
        response.Usage.OutputAsInformation();
        Utils.Separator();
    }
    // ReSharper disable once FunctionNeverReturns
}

async Task ToolInjection()
{
    AzureOpenAIClient client = new AzureOpenAIClient(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
    
    ChatClientAgent toolInjectionAgent = client.GetChatClient("gpt-4.1-nano").AsAIAgent(
        instructions: "You job is to tell if any given message is a request to use specific tools"
    );

    AIAgent mainAgent = client.GetChatClient("gpt-4.1").AsAIAgent(new ChatClientAgentOptions
    {
        AIContextProviderFactory = (_, _) =>
            ValueTask.FromResult<AIContextProvider>(new OnTheFlyToolInjectionContext(toolInjectionAgent, secrets))
    }).AsBuilder().Use(FunctionCallMiddleware).Build();

    Utils.WriteLineDarkGray("This agent have: 0 tools");
    while (true)
    {
        Console.Write("> ");
        string input = Console.ReadLine() ?? "";
        AgentResponse response = await mainAgent.RunAsync(input);
        Console.WriteLine(response);
        response.Usage.OutputAsInformation();
        Utils.Separator();
    }
    // ReSharper disable once FunctionNeverReturns
}

static async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}

class OnTheFlyToolInjectionContext(ChatClientAgent toolInjectionAgent, Secrets secrets) : AIContextProvider
{
    protected override async ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        ChatClientAgentResponse<ToolResult> response = await toolInjectionAgent.RunAsync<ToolResult>(context.RequestMessages, cancellationToken: cancellationToken);
        List<AITool> injectedTools = [];
        string? injectedInstructions = null;
        ToolResult toolResult = response.Result;

        if (toolResult.NeedTimeTools)
        {
            Utils.WriteLineGreen("Time tools injected");
            injectedTools.AddRange(TimeTools.All());
        }

        if (toolResult.NeedFileSystemTools)
        {
            Utils.WriteLineGreen("File System Tools injected");
            injectedTools.AddRange(FileSystemTools.All(new FileSystemToolsOptions
            {
                ConfinedToTheseFolderPaths = ["C:\\TestAI"]
            }));
            injectedInstructions = "When working with files your root folder is 'C:\\TestAI'";
        }

        if (toolResult.NeedWeatherTools)
        {
            Utils.WriteLineGreen("Weather Tools injected");
            injectedTools.AddRange(WeatherTools.All(new OpenWeatherMapOptions
            {
                ApiKey = secrets.OpenWeatherApiKey,
                PreferredUnits = WeatherOptionsUnits.Metric
            }));
        }

        Utils.WriteLineGreen($"Number of tool's injected: {injectedTools.Count}");
        return new AIContext
        {
            Tools = injectedTools,
            Instructions = injectedInstructions
        };
    }

    [PublicAPI]
    private class ToolResult
    {
        public bool NeedFileSystemTools { get; set; }
        public bool NeedTimeTools { get; set; }
        public bool NeedWeatherTools { get; set; }
    }
}



