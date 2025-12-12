//YouTube video that cover this sample: https://youtu.be/tDQc6lZUbYc

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using System.ClientModel;
using OpenAI;
using Microsoft.Extensions.AI;

//Start with Business as Usual
Console.Clear();
Secrets secrets = SecretManager.GetConfiguration();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClientAgent agent = client
    .GetChatClient("gpt-4.1")
    .CreateAIAgent(tools: [AIFunctionFactory.Create(GetWeather, name: "get_weather")]);

//AG-UI Part begin
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddAGUI();
WebApplication app = builder.Build();

app.MapAGUI("/", agent);

await app.RunAsync();

//Server-Tool
static string GetWeather(string city)
{
    return "It is sunny and 19 degrees";
}