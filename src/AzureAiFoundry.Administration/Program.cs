//*********************************************
//WARNING: THIS SAMPLE IS WORK IN PROGRESS
//*********************************************

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Shared;

Configuration configuration = ConfigurationManager.GetConfiguration();
PersistentAgentsClient client = new(configuration.AzureAiFoundryAgentEndpoint, new AzureCliCredential());


string? vectorStoreId = null;
try
{
    //Data + Indexes
    Response<PersistentAgentsVectorStore> vectorStore = await client.VectorStores.CreateVectorStoreAsync(name: "MyVectorStore");
}
finally
{
    if (vectorStoreId != null)
    {
        await client.VectorStores.DeleteVectorStoreAsync(vectorStoreId);
    }
}


//Agents
CancellationToken cancellationToken = new CancellationTokenSource().Token;


client.Administration.CreateAgent(
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


//await DeleteAllThreads(client);//WARNING: DELETE ALL THREADS

async Task DeleteAllThreads(PersistentAgentsClient persistentAgentsClient)
{
    await foreach (PersistentAgentThread agentThread in persistentAgentsClient.Threads.GetThreadsAsync(100))
    {
        Utils.WriteLineDarkGray("Deleting thread: " + agentThread.Id);
        await persistentAgentsClient.Threads.DeleteThreadAsync(agentThread.Id);
    }
}