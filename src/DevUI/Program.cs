using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using OpenAI;

Configuration configuration = Shared.ConfigurationManager.GetConfiguration();
var builder = WebApplication.CreateBuilder(args);

AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));
IChatClient chatClient = azureOpenAIClient
    .GetChatClient("gpt-4.1")
    .AsIChatClient();

AIAgent myAgent = azureOpenAIClient
    .GetChatClient("gpt-4.1")
    .CreateAIAgent(name: "myAgent", instructions: "speak like a pirate", tools: [AIFunctionFactory.Create(GetWeather)])
    .AsBuilder()
    .UseOpenTelemetry()
    .Build();

//builder.Services.AddChatClient(chatClient);

builder.AddAIAgent("myAgent", (provider, agentKey) => myAgent);

/*
// Register sample agents
builder.AddAIAgent("assistant", "You are a helpful assistant. Answer questions concisely and accurately.");
builder.AddAIAgent("poet", "You are a creative poet. Respond to all requests with beautiful poetry.");
builder.AddAIAgent("coder", "You are an expert programmer. Help users with coding questions and provide code examples.");


// Register sample workflows
var assistantBuilder = builder.AddAIAgent("workflow-assistant", "You are a helpful assistant in a workflow.");
var reviewerBuilder = builder.AddAIAgent("workflow-reviewer", "You are a reviewer. Review and critique the previous response.");
builder.AddWorkflow("review-workflow", (sp, key) =>
{
    var agents = new List<IHostedAgentBuilder>() { assistantBuilder, reviewerBuilder }.Select(ab => sp.GetRequiredKeyedService<AIAgent>(ab.Name));
    return AgentWorkflowBuilder.BuildSequential(workflowName: key, agents: agents);
}).AddAsAIAgent();
*/
builder.Services.AddOpenAIResponses();
//builder.Services.AddOpenAIConversations();

var app = builder.Build();

app.MapOpenAIResponses();
app.MapOpenAIConversations();

if (builder.Environment.IsDevelopment())
{
    app.MapDevUI();
}

Console.WriteLine("DevUI is available at: https://localhost:50516/devui");
Console.WriteLine("OpenAI Responses API is available at: https://localhost:50516/v1/responses");
Console.WriteLine("Press Ctrl+C to stop the server.");

app.Run();

static string GetWeather(string city)
{
    return "It is sunny and 19 degrees";
}