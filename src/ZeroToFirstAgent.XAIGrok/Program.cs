﻿//YouTube video that cover this sample: https://youtu.be/GbyEQWwBMFk

/* Steps:
 * 1: Get a Grok API Key (https://x.ai/api)
 * 2: Add Nuget Packages (OpenAI + Microsoft.Agents.AI.OpenAI)
 * 3: Create an OpenAIClient with endpoint https://api.x.ai/v1
 * 4: Get a ChatClient and Create an AI Agent from it
 * 5: Call RunAsync or RunStreamingAsync
 */

using System.ClientModel;
using Microsoft.Agents.AI;
using OpenAI;

const string apiKey = "<yourApiKey>";
const string model = "<yourModelName>";

OpenAI.OpenAIClient client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
{
    Endpoint = new Uri("https://api.x.ai/v1")
});
ChatClientAgent agent = client.GetChatClient(model).CreateAIAgent();

AgentRunResponse response = await agent.RunAsync("What is the Capital of Germany?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}