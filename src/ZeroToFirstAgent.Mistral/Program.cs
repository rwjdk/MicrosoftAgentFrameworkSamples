/* Steps:
 * 1: Get an Mistral API Key (https://admin.mistral.ai/)
 * 2: Add Nuget Packages (Mistral.SDK + Microsoft.Agents.AI)
 * 3: Create an MistralClient as an ChatClientAgent
 * 4: Call RunAsync or RunStreamingAsync (options needed for model select)
 */

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mistral.SDK;

const string apiKey = "<yourApiKey>";
const string model = Mistral.SDK.ModelDefinitions.MistralSmall;

MistralClient mistralClient = new MistralClient(new APIAuthentication(apiKey));
AIAgent agent = mistralClient.Completions.CreateAIAgent(new ChatClientAgentOptions
{
    ChatOptions = new ChatOptions
    {
        ModelId = model
    }
});

AgentRunResponse response = await agent.RunAsync("What is the Capital of Australia?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}