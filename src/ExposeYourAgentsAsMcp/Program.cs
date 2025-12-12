using Azure.AI.OpenAI;
using Shared;
using System.ClientModel;

Secrets secrets = Shared.SecretManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(client);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp("/mcp");

app.UseHttpsRedirection();

app.Run();