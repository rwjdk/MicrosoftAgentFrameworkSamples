using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using OpenAI;
using OpenAI.Chat;

#region Start Foundry and download model if needed

string modelAlias = "qwen2.5-coder-0.5b";
Console.WriteLine($"Starting AI Model '{modelAlias}'. If not already started / cached this might take a while...");
string foundryDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".foundry");

Console.WriteLine($"Using Foundry data directory: {foundryDataDir}");
CleanupStaleExecutionProviderDownloads(foundryDataDir);
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

IModel model = SelectPreferredVariant(modelFamily);
Console.WriteLine($"Selected model variant: {model.Id}");

bool isModelCached = await model.IsCachedAsync();
Console.WriteLine(isModelCached
    ? "Model already exists in the local cache."
    : "Model not cached yet. Downloading now...");

await model.DownloadAsync(progress =>
{
    Console.Write($"\rDownloading model: {progress:F2}%");
    if (progress >= 100f)
    {
        Console.WriteLine();
    }
});

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

static void CleanupStaleExecutionProviderDownloads(string foundryDataDir)
{
    string epDirectory = Path.Combine(foundryDataDir, "ep");
    if (!Directory.Exists(epDirectory))
    {
        return;
    }

    foreach (string zipPath in Directory.EnumerateFiles(epDirectory, "*.zip", SearchOption.AllDirectories))
    {
        FileInfo zipFile = new(zipPath);
        if (zipFile.Length != 0)
        {
            continue;
        }

        string? epFolder = zipFile.DirectoryName;
        if (string.IsNullOrEmpty(epFolder))
        {
            continue;
        }

        string lockFilePath = $"{epFolder}.lock";
        if (File.Exists(lockFilePath))
        {
            Console.WriteLine($"Removing stale execution provider lock: {lockFilePath}");
            File.Delete(lockFilePath);
        }

        Console.WriteLine($"Removing stale zero-byte execution provider download: {zipPath}");
        File.Delete(zipPath);
    }
}

static IModel SelectPreferredVariant(IModel modelFamily)
{
    IModel? cpuVariant = modelFamily.Variants.FirstOrDefault(variant =>
        variant.Id.Contains("generic-cpu", StringComparison.OrdinalIgnoreCase) ||
        variant.Id.Contains("cpu", StringComparison.OrdinalIgnoreCase));

    return cpuVariant ?? modelFamily;
}
