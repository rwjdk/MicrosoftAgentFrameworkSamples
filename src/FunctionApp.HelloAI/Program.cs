using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AzureFunctions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using OpenAI;

//Start storage docker
// docker run -d --name storage-emulator -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

Shared.Secrets secrets = Shared.SecretManager.GetSecrets();

AzureOpenAIClient azureOpenAIClient = new(
    new Uri(secrets.AzureOpenAiEndpoint),
    new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClientAgent myAgent = azureOpenAIClient
    .GetChatClient("gpt-4.1-mini")
    .CreateAIAgent(name: "MyAgent");

//From nuget: Microsoft.Agents.AI.Hosting.AzureFunctions
builder.ConfigureDurableAgents(options => options.AddAIAgent(myAgent));

builder.Build().Run();