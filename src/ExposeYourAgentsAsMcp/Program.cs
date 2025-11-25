using Azure.AI.OpenAI;
using Shared;
using System.ClientModel;

Configuration configuration = Shared.ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(client);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp("/mcp");

app.UseHttpsRedirection();

app.Run();