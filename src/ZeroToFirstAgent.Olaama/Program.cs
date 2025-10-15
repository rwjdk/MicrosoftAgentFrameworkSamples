//YouTube video that cover this sample: https://youtu.be/GbyEQWwBMFk

/* Steps:
 * 1: Download Ollama + Model (https://ollama.com/)
 * 2: Add Nuget Packages (OllamaSharp + Microsoft.Agents.AI)
 * 3: Create an OllamaApiClient and store it as IChatClient for an ChatClientAgent
 * 4: Call RunAsync or RunStreamingAsync
 */

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

IChatClient client = new OllamaApiClient("http://localhost:11434", "llama3.2:1b");
ChatClientAgent agent = new(client);
AgentRunResponse response = await agent.RunAsync("What is the Capital of Sweden?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}