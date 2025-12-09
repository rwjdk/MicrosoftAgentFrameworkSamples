//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

#pragma warning disable OPENAI001
using A2A;
using Azure.AI.OpenAI;
using GenerativeAI;
using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Responses;
using Playground;
using Shared;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.IO;
using System.Text;
using System.Text.Json;

Console.WriteLine("");
Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();

OpenAIClient openAiClient = new OpenAIClient(configuration.OpenAiApiKey);
AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClientAgent a = openAiClient.GetOpenAIResponseClient("gpt-4.1").CreateAIAgent();
List<ChatMessage> chatMessages = new List<ChatMessage>();
chatMessages.Add(new ChatMessage(ChatRole.User, []));
await a.RunAsync(chatMessages);

ServiceCollection services = new();
services.AddScoped<HttpClient>();
services.AddScoped<ToolWithDiNeed>();
IServiceProvider serviceProvider = services.BuildServiceProvider();

ChatClientAgent agent = azureOpenAIClient
    .GetChatClient("gpt-4.1")
    .CreateAIAgent(
        tools: [AIFunctionFactory.Create(serviceProvider.GetRequiredService<ToolWithDiNeed>().GetNews)],
        services: serviceProvider);

AgentRunResponse response = await agent.RunAsync("Get me the news");
Console.WriteLine(response);

//await AzureOpenAiFoundry.Run(configuration);
//await FileTool.Run(configuration);
//await CodeTool.Run(configuration);
//await ReasoningSummary.Run(configuration);
//await CodexSpecialModels.Run(configuration);
//await SpaceNewsWebSearch.Run(configuration);
//await ResumeConversation.Run(configuration);
//await AzureOpenAiCodex.Run(configuration);
