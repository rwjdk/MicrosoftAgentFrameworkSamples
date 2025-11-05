// Create root command with options

using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using Agent2Agent.Client;

await Task.Delay(5000);

RootCommand rootCommand = new RootCommand("A2AClient");
rootCommand.SetAction((_, token) => HandleCommandsAsync(token));

// Run the command
return await rootCommand.Parse(args).InvokeAsync();

static async Task HandleCommandsAsync(CancellationToken cancellationToken)
{
    // Set up the logging
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    });
    var logger = loggerFactory.CreateLogger("A2AClient");

    var agentUrls = "http://localhost:5000/";

    // Create the Host agent
    var hostAgent = new HostClientAgent(loggerFactory);
    await hostAgent.InitializeAgentAsync(agentUrls!.Split(";"));
    AgentThread thread = hostAgent.Agent!.GetNewThread();
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

            var agentResponse = await hostAgent.Agent!.RunAsync(message, thread, cancellationToken: cancellationToken);
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