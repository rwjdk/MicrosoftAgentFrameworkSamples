using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using Trello.Agent;
using Trello.Agent.Models;

Console.Clear();
Configuration configuration = ConfigurationManager.GetConfiguration();

HostApplicationBuilder builder = new();
builder.Services.AddSingleton(new AzureOpenAIClient(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey)));
builder.Services.AddSingleton(new TrelloCredentials(configuration.TrelloApiKey, configuration.TrelloToken));
builder.Services.AddSingleton<AgentFactory>();

IHost host = builder.Build();
AgentFactory agentFactory = host.Services.GetRequiredService<AgentFactory>();

ChatClientAgent orchestratorAgent = agentFactory.GetOrchestratorAgent();
ChatClientAgent trelloAgent = await agentFactory.GetTrelloAgent();

while (true)
{
    List<ChatMessage> messages = [];
    Workflow workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(orchestratorAgent)
        .WithHandoffs(orchestratorAgent, [trelloAgent])
        .WithHandoffs([trelloAgent], orchestratorAgent)
        .Build();
    Console.Write("> ");
    messages.Add(new(ChatRole.User, Console.ReadLine()!));
    messages.AddRange(await RunWorkflowAsync(workflow, messages));
}

static async Task<List<ChatMessage>> RunWorkflowAsync(Workflow workflow, List<ChatMessage> messages)
{
    string? lastExecutorId = null;

    StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
    await foreach (WorkflowEvent @event in run.WatchStreamAsync())
    {
        switch (@event)
        {
            case AgentRunUpdateEvent e:
            {
                if (e.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = e.ExecutorId;
                    Console.WriteLine();
                    Utils.WriteLineGreen(e.Update.AuthorName ?? e.ExecutorId);
                }

                Console.Write(e.Update.Text);
                if (e.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is FunctionCallContent call)
                {
                    Console.WriteLine();
                    Utils.WriteLineDarkGray($"Call '{call.Name}' with arguments: {JsonSerializer.Serialize(call.Arguments)}]");
                }

                break;
            }
            case WorkflowOutputEvent output:
                Utils.Separator();
                return output.As<List<ChatMessage>>()!;
            case ExecutorFailedEvent failedEvent:
                if (failedEvent.Data is Exception ex)
                {
                    Utils.WriteLineRed($"Error in agent {failedEvent.ExecutorId}: " + ex);
                }

                break;
        }
    }

    return [];
}