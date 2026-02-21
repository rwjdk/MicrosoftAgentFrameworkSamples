//YouTube video that cover this sample: https://youtu.be/GbyEQWwBMFk

/* Steps:
 * 1: Get an Anthropic API Key (https://docs.claude.com/en/api/admin-api/apikeys/get-api-key)
 * 2: Add Nuget Packages (Anthropic.SDK + Microsoft.Agents.AI)
 * 3: Create an AnthropicClient for an ChatClientAgent
 * 4: Call RunAsync or RunStreamingAsync (options needed for model select)
 */

using Anthropic;
using Anthropic.Core;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

const string apiKey = "todo";
const string model = "todo";
AnthropicClient client = new AnthropicClient(new ClientOptions
{
    ApiKey = apiKey
});
ChatClientAgent agent = client.AsAIAgent(new ChatClientAgentOptions
{
    ChatOptions = new ChatOptions { ModelId = model, MaxOutputTokens = 1000 }
});


AgentResponse response = await agent.RunAsync("What is the Capital of Australia?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}