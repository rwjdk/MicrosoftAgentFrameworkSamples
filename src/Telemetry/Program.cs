//YouTube video that cover this sample: https://youtu.be/jeVQo75KcCw

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenTelemetry;
using Shared;
using System.ClientModel;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenAI.Chat;
using OpenTelemetry.Trace;

Secrets secrets = SecretManager.GetSecrets();

//Setup Telemetry
string sourceName = "AiSource";
var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)
    .AddConsoleExporter();
if (!string.IsNullOrWhiteSpace(secrets.ApplicationInsightsConnectionString))
{
    tracerProviderBuilder.AddAzureMonitorTraceExporter(options => options.ConnectionString = secrets.ApplicationInsightsConnectionString);
}

using TracerProvider tracerProvider = tracerProviderBuilder.Build();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

AIAgent agent = client
    .GetChatClient(secrets.ChatDeploymentName)
    .AsAIAgent(
        name: "MyObservedAgent",
        instructions: "You are a Friendly AI Bot, answering questions")
    .AsBuilder()
    .UseOpenTelemetry(sourceName, options =>
    {
        options.EnableSensitiveData = true; //If the actual messages should be logged or not
    })
    .Build();

AgentThread thread = await agent.GetNewThreadAsync();

AgentResponse response1 = await agent.RunAsync("Hello, My name is Rasmus", thread);
Utils.WriteLineRed(response1.Text);
Utils.Separator();

AgentResponse response2 = await agent.RunAsync("What is the capital of France?", thread);
Utils.WriteLineRed(response2.Text);
Utils.Separator();

AgentResponse response3 = await agent.RunAsync("What was my name?", thread);
Utils.WriteLineRed(response3.Text);
Utils.Separator();