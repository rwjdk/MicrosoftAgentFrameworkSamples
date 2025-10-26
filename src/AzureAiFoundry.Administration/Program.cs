//YouTube video that cover this sample: https://youtu.be/V2piYocabI8

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Shared;

Configuration configuration = ConfigurationManager.GetConfiguration();
PersistentAgentsClient client = new(configuration.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

PersistentAgentsFiles files = client.Files; //Files CRUD

VectorStores vectorStores = client.VectorStores; //Vector Store CRUD + Add/Remove Files to/from them

Threads threads = client.Threads; //Thread CRUD

ThreadMessages messages = client.Messages; //Messages CRUD (associated with Threads)

ThreadRuns runs = client.Runs; //The Invocation of asking the LLM (+ stats on it)

PersistentAgentsAdministrationClient administration = client.Administration; //The Agents CRUD

//Agents Details
CancellationToken cancellationToken = new CancellationTokenSource().Token;

Response<PersistentAgent> existingPersistentAgent = await administration.GetAgentAsync("asst_44Z3VVpGZXuD91HRKohhvC68");
ChatClientAgent afAgentFromExisting = await client.GetAIAgentAsync(existingPersistentAgent.Value.Id);


Response<PersistentAgent> newPersistentAgent = administration.CreateAgent(
    model: "gpt-4.1-mini",
    name: "NameOfClient",
    description: "DescriptionOfClient",
    instructions: "Instructions for LLM",
    toolResources: new ToolResources
    {
        AzureAISearch = new AzureAISearchToolResource(indexConnectionId: "", indexName: "", topK: 1, filter: "", queryType: AzureAISearchQueryType.Vector),
        CodeInterpreter = new CodeInterpreterToolResource
        {
            DataSources = { },
            FileIds = { }
        },
        FileSearch = new FileSearchToolResource
        {
            VectorStoreIds = { },
            VectorStores = { }
        },
        Mcp =
        {
            new MCPToolResource(serverLabel: ""),
            new MCPToolResource(serverLabel: ""),
        }
    },
    tools: new List<ToolDefinition>
    {
        new CodeInterpreterToolDefinition()
    },
    temperature: 1, //NB: Do not touch these, if you use a reason model
    topP: 1, //NB: Do not touch these, if you use a reason model
    responseFormat: BinaryData.Empty,
    metadata: new Dictionary<string, string>
    {
    },
    cancellationToken: cancellationToken);

ChatClientAgent afAgentFromNew = await client.GetAIAgentAsync(newPersistentAgent.Value.Id);

//Example:
//await DeleteAllThreads(client);//WARNING: DELETE ALL THREADS

async Task DeleteAllThreads(PersistentAgentsClient persistentAgentsClient)
{
    await foreach (PersistentAgentThread agentThread in persistentAgentsClient.Threads.GetThreadsAsync(100))
    {
        Utils.WriteLineDarkGray("Deleting thread: " + agentThread.Id);
        await persistentAgentsClient.Threads.DeleteThreadAsync(agentThread.Id);
    }
}