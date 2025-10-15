//YouTube video that cover this sample: https://youtu.be/GbyEQWwBMFk

/* Steps:
 * 1: Run the code (Every set itself up)
 */

using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.AI.Foundry.Local;
using OpenAI;
using System.Diagnostics;

#region Check if Foundry Local is installed

string packageId = "Microsoft.FoundryLocal";
Process process = new()
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "winget",
        Arguments = $"list --id={packageId}",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    }
};
process.Start();
string output = process.StandardOutput.ReadToEnd();
process.WaitForExit();
bool isFoundryInstalled = output.Contains(packageId, StringComparison.OrdinalIgnoreCase);

#endregion

#region Install Part (if needed)

if (!isFoundryInstalled)
{
    Console.WriteLine("Foundry Local not yet installed. Installing... (this might take a few minutes)");

    Process installProcess = new()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "winget",
            Arguments = "install Microsoft.FoundryLocal --accept-package-agreements --accept-source-agreements --silent",
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };
    installProcess.Start();
    installProcess.WaitForExit();
}

#endregion

#region Start Foundry and download model if needed

string modelAlias = "qwen2.5-coder-0.5b";
Console.WriteLine($"Starting AI Model '{modelAlias}'. If not already started / cached this might take a while...");
FoundryLocalManager manager = await FoundryLocalManager.StartModelAsync(modelAlias);
ModelInfo? modelInfo = await manager.GetModelInfoAsync(modelAlias);

#endregion

OpenAIClient client = new(new ApiKeyCredential("NO_API_KEY"), new OpenAIClientOptions
{
    Endpoint = manager.Endpoint
});
ChatClientAgent agent = client.GetChatClient(modelInfo!.ModelId).CreateAIAgent();

AgentRunResponse response = await agent.RunAsync("What is the Capital of Sweden?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}