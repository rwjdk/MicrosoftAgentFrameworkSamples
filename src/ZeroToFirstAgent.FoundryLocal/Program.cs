using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using OpenAI;
using OpenAI.Chat;
using Shared;

#region Start Foundry and download model if needed

Utils.Init("Foundry.Local Sample");

string modelAlias = "qwen2.5-coder-0.5b";
Console.WriteLine($"Starting AI Model '{modelAlias}'. If not already started / cached this might take a while...");
string foundryDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".foundry");

Console.WriteLine($"Using Foundry data directory: {foundryDataDir} (Try removing this folder if code hangs, or cause Exceptions");
Console.WriteLine("Initializing Foundry Local SDK...");
await FoundryLocalManager.CreateAsync(
    new Configuration
    {
        AppName = "ZeroToFirstAgentFoundryLocal",
        AppDataDir = foundryDataDir,
        Web = new Configuration.WebService
        {
            Urls = "http://127.0.0.1:0"
        }
    },
    NullLogger.Instance);

FoundryLocalManager manager = FoundryLocalManager.Instance;

ICatalog catalog = await manager.GetCatalogAsync();
IModel modelFamily = await catalog.GetModelAsync(modelAlias) ?? throw new InvalidOperationException($"Model '{modelAlias}' not found.");


IModel model = modelFamily.Variants.First(v => v.Info.Runtime?.DeviceType == DeviceType.GPU); 
Console.WriteLine($"Selected model variant: {model.Id}");

bool isModelCached = await model.IsCachedAsync();
if (!isModelCached)
{
    Console.WriteLine("Model not yet cached yet. Downloading...");
    await model.DownloadAsync();
}

Console.WriteLine("Loading model into memory...");
await model.LoadAsync();

Console.WriteLine("Starting local OpenAI-compatible web service...");
await manager.StartWebServiceAsync();

#endregion

string serviceUrl = manager.Urls?.FirstOrDefault()
    ?? throw new InvalidOperationException("Foundry Local did not report a web service URL.");
Uri endpoint = new($"{serviceUrl}/v1");
OpenAIClient client = new(new ApiKeyCredential("NO_API_KEY"), new OpenAIClientOptions
{
    Endpoint = endpoint
});
ChatClientAgent agent = client.GetChatClient(model.Id).AsAIAgent();

AgentResponse response = await agent.RunAsync("What is the Capital of Sweden?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}

await manager.StopWebServiceAsync();
await model.UnloadAsync();