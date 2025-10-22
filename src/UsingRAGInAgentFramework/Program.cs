//YouTube video that cover this sample: https://youtu.be/Vpi5aZJRJmA

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using UsingRAGInAgentFramework.Models;

//Prep
string jsonWithMovies = await File.ReadAllTextAsync("made_up_movies.json");
Movie[] movieDataForRag = JsonSerializer.Deserialize<Movie[]>(jsonWithMovies)!;

ChatMessage question = new(ChatRole.User, "What is the 3 highest rated adventure movies (list their titles, plots and ratings)");

Configuration configuration = ConfigurationManager.GetConfiguration();
AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClientAgent agent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(instructions: "You are an expert a set of made up movies given to you (aka don't consider movies from your world-knowledge)");

#region Let's give the model all data upfront

Utils.WriteLineGreen("Sample 1 - Preload all data");
List<ChatMessage> preloadEverythingChatMessages =
[
    new(ChatRole.Assistant, "Here are all the movies")
];
foreach (Movie movie in movieDataForRag)
{
    preloadEverythingChatMessages.Add(new ChatMessage(ChatRole.Assistant, movie.GetTitleAndDetails()));
}

preloadEverythingChatMessages.Add(question);

AgentRunResponse response1 = await agent.RunAsync(preloadEverythingChatMessages);
Console.WriteLine(response1);
response1.Usage.OutputAsInformation();

#endregion

Console.Clear();

#region That is too expensive!... Let's use RAG

Utils.WriteLineGreen("Sample 2 - Preload RAG Data");

Microsoft.Extensions.AI.IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = client
    .GetEmbeddingClient("text-embedding-3-small")
    .AsIEmbeddingGenerator();

Microsoft.SemanticKernel.Connectors.InMemory.InMemoryVectorStore vectorStore =
    new(new InMemoryVectorStoreOptions
    {
        EmbeddingGenerator = embeddingGenerator
    });

//Microsoft.SemanticKernel.Connectors.AzureAISearch.AzureAISearchVectorStore vectorStoreFromAzureAiSearch = new AzureAISearchVectorStore(
//    new SearchIndexClient(new Uri("azureAiSearchEndpoint"),
//        new AzureKeyCredential("azureAiSearchKey")
//    ));

//Microsoft.SemanticKernel.Connectors.SqlServer.SqlServerVectorStore vectorStoreFromSqlServer2025 = new SqlServerVectorStore(
//    "connectionString");

//Microsoft.SemanticKernel.Connectors.CosmosNoSql.CosmosNoSqlVectorStore vectorStoreFromCosmosDb = new CosmosNoSqlVectorStore(
//    "connectionString",
//    "databaseName",
//    new CosmosClientOptions
//    {
//        UseSystemTextJsonSerializerWithOptions = JsonSerializerOptions.Default,
//    });

InMemoryCollection<Guid, MovieVectorStoreRecord> collection = vectorStore.GetCollection<Guid, MovieVectorStoreRecord>("movies");
await collection.EnsureCollectionExistsAsync();

int counter = 0;
foreach (Movie movie in movieDataForRag)
{
    counter++;
    Console.Write($"\rEmbedding Movies: {counter}/{movieDataForRag.Length}");
    await collection.UpsertAsync(new MovieVectorStoreRecord
    {
        Id = Guid.NewGuid(),
        Title = movie.Title,
        Plot = movie.Plot,
        Rating = movie.Rating
    });
}

Console.WriteLine();
Console.WriteLine("\rEmbedding complete... Let's as the question again using RAG");

List<ChatMessage> ragPreloadChatMessages =
[
    new(ChatRole.Assistant, "Here are the most relevant movies")
];
await foreach (VectorSearchResult<MovieVectorStoreRecord> searchResult in collection.SearchAsync(question.Text, 10,
                   new VectorSearchOptions<MovieVectorStoreRecord>
                   {
                       IncludeVectors = false
                   }))
{
    MovieVectorStoreRecord record = searchResult.Record;
    ragPreloadChatMessages.Add(new ChatMessage(ChatRole.Assistant, record.GetTitleAndDetails()));
}

ragPreloadChatMessages.Add(question);

AgentRunResponse response2 = await agent.RunAsync(ragPreloadChatMessages);
Console.WriteLine(response2);
response2.Usage.OutputAsInformation();

#endregion

Console.Clear();

#region Let's do the same as above but as function calling [Smart if the use example just say 'Hello' we do not preload any movies]

Utils.WriteLineGreen("Sample 3 RAG via Tool Call");

SearchTool searchTool = new(collection);

AIAgent agentWithTools = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(
        instructions: "You are an expert a set of made up movies given to you (aka don't consider movies from your world-knowledge)",
        tools: [AIFunctionFactory.Create(searchTool.SearchVectorStore)]
    ).AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

AgentRunResponse response3 = await agentWithTools.RunAsync(question);
Console.WriteLine(response3);
response3.Usage.OutputAsInformation();

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}

class SearchTool(InMemoryCollection<Guid, MovieVectorStoreRecord> collection)
{
    public async Task<List<string>> SearchVectorStore(string question)
    {
        List<string> result = [];
        await foreach (VectorSearchResult<MovieVectorStoreRecord> searchResult in collection.SearchAsync(question, 10,
                           new VectorSearchOptions<MovieVectorStoreRecord>
                           {
                               IncludeVectors = false
                           }))
        {
            MovieVectorStoreRecord record = searchResult.Record;
            result.Add(record.GetTitleAndDetails());
        }

        return result;
    }
}

#endregion