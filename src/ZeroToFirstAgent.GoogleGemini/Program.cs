//YouTube video that cover this sample: https://youtu.be/GbyEQWwBMFk

/* Steps:
 * 1: Get a Google API Gemini API Key (https://aistudio.google.com/app/api-keys)
 * 2: Add Nuget Packages (Google.GenAI + Microsoft.Agents.AI)
 * 3: Create an Google.GenAI.Client for an ChatClientAgent
 * 4: Call RunAsync or RunStreamingAsync
 */

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

const string apiKey = "<yourApiKey>";
const string model = "gemini-3-pro-preview";

IChatClient client = new Google.GenAI.Client(apiKey: apiKey).AsIChatClient(model);
ChatClientAgent agent = new(client);

AgentResponse response = await agent.RunAsync("What is the Capital of Australia?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}