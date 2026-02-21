//YouTube video that cover this sample: https://youtu.be/aQD4vhzQRvI

/* Steps:
 * 1: Create an 'Azure AI Foundry' Resource (or legacy 'Azure OpenAI Resource') + Deploy Model
 * 2: Add Nuget Packages (Azure.AI.OpenAI + Microsoft.Agents.AI.OpenAI)
 * 3: Create an AzureOpenAIClient (API Key or Azure Identity)
 * 4: Get a ChatClient and Create an AI Agent from it
 * 5: Call RunAsync or RunStreamingAsync
 */

using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

const string endpoint = "todo";
const string apiKey = "todo";
const string model = "todo";
AzureOpenAIClient client = new(new Uri(endpoint), new ApiKeyCredential(apiKey));
ChatClientAgent agent = client.GetChatClient(model).AsAIAgent();

//Simple Response
AgentResponse response = await agent.RunAsync("What is the capital of France?");
Console.WriteLine(response);

Console.WriteLine("---");

//Streaming Result
await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}