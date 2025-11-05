// Create root command with options

using A2A;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Shared;
using System.ClientModel;
using System.CommandLine;
using OpenAI;

await Task.Delay(5000);
RootCommand rootCommand = new RootCommand("A2AClient");
rootCommand.SetAction((_, token) => HandleCommandsAsync(token));

// Run the command
return await rootCommand.Parse(args).InvokeAsync();

static async Task HandleCommandsAsync(CancellationToken cancellationToken)
{
    Configuration configuration = Shared.ConfigurationManager.GetConfiguration();
    // Set up the logging
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    });
    var logger = loggerFactory.CreateLogger("A2AClient");

    var agentUrl = "http://localhost:5000/";

    AIAgent remoteAgent = await CreateRemoteAgentAsync(agentUrl);
    List<AITool> tools = [remoteAgent.AsAIFunction()];

    // Create the agent that uses the remote agents as tools
    ChatClientAgent clientAgent = new AzureOpenAIClient(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey))
        .GetChatClient("gpt-4.1")
        .CreateAIAgent(instructions: "You specialize in handling queries for users and using your tools to provide answers.", name: "HostClient", tools: tools);

    AgentThread thread = clientAgent.GetNewThread();
    try
    {
        while (true)
        {
            // Get user message
            Console.Write("\nUser (:q or quit to exit): ");
            string? message = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("Request cannot be empty.");
                continue;
            }

            if (message is ":q" or "quit")
            {
                break;
            }

            var agentResponse = await clientAgent.RunAsync(message, thread, cancellationToken: cancellationToken);
            foreach (var chatMessage in agentResponse.Messages)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nAgent: {chatMessage.Text}");
                Console.ResetColor();
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while running the A2AClient");
        return;
    }
}

static async Task<AIAgent> CreateRemoteAgentAsync(string agentUri)
{
    Uri url = new Uri(agentUri);
    HttpClient httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    A2ACardResolver agentCardResolver = new A2ACardResolver(url, httpClient);

    try
    {
        return await agentCardResolver.GetAIAgentAsync();
    }
    catch (Exception e)
    {
        Utils.WriteLineRed(e);
        throw;
    }
}