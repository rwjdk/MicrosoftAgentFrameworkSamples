//YouTube video that cover this sample: https://youtu.be/DoyeSZqim08

/* Steps:
 * 1: Create an 'Azure AI Foundry' Resource + Deploy Model
 * 2: Add Nuget Packages (Azure.AI.Agents.Persistent, Azure.Identity, Microsoft.Agents.AI.AzureAI)
 * 3: Create an PersistentAgentsClient (Azure Identity)
 * 4: Use the client's Administration to create a new agent
 * 5: Use client to get an ChatClientAgent from the persistentAgent's Id
 * 6: Create a new Thread
 * 7: Call like normal
 * 8: (Optional) Clean up
 */

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;

const string endpoint = "<Azure AI Foundry project endpoint>";
const string model = "<your model>";

PersistentAgentsClient client = new(endpoint, new AzureCliCredential());

Response<PersistentAgent>? aiFoundryAgent = null;
try
{
    aiFoundryAgent = await client.Administration.CreateAgentAsync(model, "MyFirstAgent", "Some description", "You are a nice AI");

    ChatClientAgent agent = await client.GetAIAgentAsync(aiFoundryAgent.Value.Id);

    AgentThread thread = agent.GetNewThread();

    AgentRunResponse response = await agent.RunAsync("What is the capital of France?", thread);
    Console.WriteLine(response);

    Console.WriteLine("---");

    await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?", thread))
    {
        Console.Write(update);
    }
}
finally
{
    if (aiFoundryAgent != null)
    {
        await client.Administration.DeleteAgentAsync(aiFoundryAgent.Value.Id);
    }
}