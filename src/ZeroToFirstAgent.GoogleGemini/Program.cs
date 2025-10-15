//YouTube video that cover this sample: https://youtu.be/GbyEQWwBMFk

/* Steps:
 * 1: Get a Google API Gemini API Key (https://aistudio.google.com/app/api-keys)
 * 2: Add Nuget Packages (Google_GenerativeAI.Microsoft + Microsoft.Agents.AI)
 * 3: Create an GenerativeAIChatClient for an ChatClientAgent
 * 4: Call RunAsync or RunStreamingAsync
 */

using GenerativeAI;
using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

const string apiKey = "<yourApiKey>";
const string model = GoogleAIModels.Gemini25Pro;

IChatClient client = new GenerativeAIChatClient(apiKey, model);
ChatClientAgent agent = new(client);

AgentRunResponse response = await agent.RunAsync("What is the Capital of Australia?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}