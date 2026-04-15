using Azure.AI.OpenAI;
using Shared;
using System.ClientModel;

Secrets secrets = Shared.SecretsManager.GetSecrets();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(client);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

WebApplication app = builder.Build();

app.MapMcp("/mcp");

app.UseHttpsRedirection();

app.Run();