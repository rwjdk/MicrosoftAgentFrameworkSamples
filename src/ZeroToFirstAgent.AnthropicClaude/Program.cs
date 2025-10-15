//YouTube video that cover this sample: https://youtu.be/GbyEQWwBMFk

/* Steps:
 * 1: Get an Anthropic API Key (https://docs.claude.com/en/api/admin-api/apikeys/get-api-key)
 * 2: Add Nuget Packages (Anthropic.SDK + Microsoft.Agents.AI)
 * 3: Create an AnthropicClient for an ChatClientAgent
 * 4: Call RunAsync or RunStreamingAsync (options needed for model select)
 */

using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

const string apiKey = "<yourApiKey>";
const string model = AnthropicModels.Claude35Haiku;

IChatClient client = new AnthropicClient(new APIAuthentication(apiKey)).Messages.AsBuilder().Build();

ChatClientAgentRunOptions chatClientAgentRunOptions = new(new()
{
    ModelId = model,
    MaxOutputTokens = 1000
});

ChatClientAgent agent = new(client);
AgentRunResponse response = await agent.RunAsync("What is the Capital of Australia?", options: chatClientAgentRunOptions);
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?", options: chatClientAgentRunOptions))
{
    Console.Write(update);
}