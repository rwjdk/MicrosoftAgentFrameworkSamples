﻿/* Steps:
 * 1: Get a OpenRouter API Key (https://openrouter.ai/settings/keys)
 * 2: Add Nuget Packages (OpenAI + Microsoft.Agents.AI.OpenAI)
 * 3: Create an OpenAIClient with endpoint https://openrouter.ai/api/v1
 * 4: Get a ChatClient and Create an AI Agent from it
 * 5: Call RunAsync or RunStreamingAsync
 */

using System.ClientModel;
using Microsoft.Agents.AI;
using OpenAI;

const string apiKey = "<YourApiKey>";
const string model = "<yourModelName>";

OpenAI.OpenAIClient client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
{
    Endpoint = new Uri("https://openrouter.ai/api/v1")
});
AIAgent agent = client.GetChatClient(model).CreateAIAgent();

AgentRunResponse response = await agent.RunAsync("What is the Capital of Norway?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}