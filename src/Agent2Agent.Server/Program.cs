using A2A;
using A2A.AspNetCore;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.ClientModel;
using OpenAI;
using Shared;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
var app = builder.Build();

Configuration configuration = Shared.ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

AIAgent agent = client
    .GetChatClient("gpt-4.1-mini")
    .CreateAIAgent(
        name: "FileAgent",
        instructions: "You report on files and folders",
        tools: [AIFunctionFactory.Create(NumberOfFiles)]
    );
AgentCard card = GetServerAgentCard();

app.MapA2A(
    agent,
    path: "/",
    agentCard: card,
    taskManager => app.MapWellKnownAgentCard(taskManager, "/"));

await app.RunAsync();


Console.WriteLine("Hello, World!");


static AgentCard GetServerAgentCard()
{
    AgentCapabilities capabilities = new AgentCapabilities()
    {
        Streaming = false,
        PushNotifications = false,
    };

    AgentSkill skill = new AgentSkill()
    {
        Id = "my_files_agent",
        Name = "File Expert",
        Description = "Handles requests relating to files on hard disk",
        Tags = ["files", "folders"],
        Examples =
        [
            "What files are the in Folder 'Demo1'",
        ],
    };

    return new()
    {
        Name = "FilesAgent",
        Description = "Handles requests relating to files",
        Version = "1.0.0",
        DefaultInputModes = ["text"],
        DefaultOutputModes = ["text"],
        Capabilities = capabilities,
        Skills = [skill],
        Url = "http://localhost:5000"
    };
}

static int NumberOfFiles()
{
    return 324342342;
}