//YouTube video that cover this sample: https://youtu.be/CvA69UyqJ7U

/* Steps:
 * 1: Create an 'OpenAI API Account'
 * 2: Add Nuget Packages (OpenAI + Microsoft.Agents.AI.OpenAI)
 * 3: Create an OpenAIClient
 * 4: Get a ChatClient and Create an AI Agent from it
 * 5: Call RunAsync or RunStreamingAsync
 */

using Microsoft.Agents.AI;
using OpenAI;

const string apiKey = "<YourAPIKey>";
const string model = "<yourModelName>"; //Example: gpt-4.1

OpenAIClient client = new(apiKey);
ChatClientAgent agent = client.GetChatClient(model).CreateAIAgent();

AgentRunResponse response = await agent.RunAsync("What is the Capital of Germany?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}