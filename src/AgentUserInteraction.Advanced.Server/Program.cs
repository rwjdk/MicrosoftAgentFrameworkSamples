using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using AgentUserInteraction.Advanced.Server.AgUiSpecializedAgents;
using AgentUserInteraction.Advanced.SharedModels;
using OpenAI.Chat;

Secrets secrets = SecretManager.GetSecrets();
AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

string model = "gpt-4.1-mini";

ChatClientAgent clientToolAgent = client
    .GetChatClient(model)
    .AsAIAgent(instructions: "You are a nice AI");

ChatClientAgent weatherAgent = client
    .GetChatClient(model)
    .AsAIAgent(instructions: "You Report the Weather.",
        tools: [AIFunctionFactory.Create(GetWeather, name: "get_weather")]);

ChatClientAgent weatherAgentStructured = client
    .GetChatClient(model)
    .AsAIAgent(instructions: "Always call tool 'get_weather' first. Then tell a story about the city but leave out the weather details",
        tools: [AIFunctionFactory.Create(GetWeather, name: "get_weather")]);

ChatClientAgent movieStructuredOutputAgent = client
    .GetChatClient(model)
    .AsAIAgent(instructions: "You are a movie Expert");


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAGUI();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Cors",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


WebApplication app = builder.Build();

app.UseCors("Cors");

app.MapAGUI("/clientToolAgent", clientToolAgent);
app.MapAGUI("/weatherAgent", weatherAgent);
app.MapAGUI("/weatherAgentWithStructuredContent", new AgUiStructuredToolsOutputAgent(weatherAgentStructured, "get_weather"));
app.MapAGUI("/movieAgent", new AgUiStructuredOutputAgent<MovieResult>(movieStructuredOutputAgent));

app.UseHttpsRedirection();
app.Run();

//Server-Tool
static WeatherReport GetWeather(string city)
{
    return new WeatherReport
    {
        City = city,
        Condition = "Sunny",
        DegreesC = 25,
        DegreesF = 77
    };
}