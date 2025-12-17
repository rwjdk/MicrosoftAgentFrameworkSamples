using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using Shared;

Console.Clear();
Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(
    new Uri(secrets.AzureOpenAiEndpoint),
    new ApiKeyCredential(secrets.AzureOpenAiKey));

ServiceCollection services = new();
services.AddScoped<HttpClient>();
services.AddScoped<ToolClass1>();
services.AddScoped<ToolClass2>();
IServiceProvider serviceProvider = services.BuildServiceProvider();

ToolClass1 toolClass1Instance = serviceProvider.GetRequiredService<ToolClass1>();
ToolClass2 toolClass2Instance = serviceProvider.GetRequiredService<ToolClass2>();

#region Agent Part

ChatClientAgent agent = azureOpenAIClient
    .GetChatClient("gpt-4.1")
    .CreateAIAgent(
        new ChatClientAgentOptions
        {
        },
        tools:
        [
            AIFunctionFactory.Create(StaticTool, "tool1"),
            AIFunctionFactory.Create(toolClass1Instance.ToolInToolClass, "tool2"),
            AIFunctionFactory.Create(ToolClass2.ToolInToolClass, "tool3"),
            AIFunctionFactory.Create(toolClass2Instance.ToolInToolClassInstance, "tool4")
        ],
        services: serviceProvider);

AgentRunResponse response = await agent.RunAsync("Call Tool1");
Console.WriteLine(response);

response = await agent.RunAsync("Call Tool2");
Console.WriteLine(response);

response = await agent.RunAsync("Call Tool3");
Console.WriteLine(response);

response = await agent.RunAsync("Call Tool4");
Console.WriteLine(response);

#endregion

static string StaticTool()
{
    return "Say 'I'm a static tool to the user";
}

class ToolClass1(HttpClient httpClient)
{
    public string ToolInToolClass()
    {
        return "Say 'I'm a tool in a tool class";
    }
}

class ToolClass2
{
    public static string ToolInToolClass(IServiceProvider serviceProvider)
    {
        HttpClient httpClient = serviceProvider.GetRequiredService<HttpClient>();
        return "Say 'I'm a static tool that need an HTTP Client";
    }

    public string ToolInToolClassInstance(IServiceProvider serviceProvider)
    {
        HttpClient httpClient = serviceProvider.GetRequiredService<HttpClient>();
        return "Say 'I'm an instance tool that need an HTTP Client";
    }
}