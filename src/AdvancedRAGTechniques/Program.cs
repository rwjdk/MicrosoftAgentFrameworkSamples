//YouTube video that cover this sample: https://youtu.be/0Lt1rmOEZNs

using AdvancedRAGTechniques;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Shared;
using System.ClientModel;
using System.Text.Json;
using UsingRAGInAgentFramework.Models;

Console.Clear();
//Prep + Embedding
string jsonWithMovies = await File.ReadAllTextAsync("made_up_movies.json");
Movie[] movieDataForRag = JsonSerializer.Deserialize<Movie[]>(jsonWithMovies)!;

Configuration configuration = ConfigurationManager.GetConfiguration();
AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

Microsoft.Extensions.AI.IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = client
    .GetEmbeddingClient("text-embedding-3-small")
    .AsIEmbeddingGenerator();

//Microsoft.SemanticKernel.Connectors.InMemory.InMemoryVectorStore vectorStore =
//    new(new InMemoryVectorStoreOptions
//    {
//        EmbeddingGenerator = embeddingGenerator
//    });

Microsoft.SemanticKernel.Connectors.SqlServer.SqlServerVectorStore vectorStore = new SqlServerVectorStore(
    "Server=(local);Database=RAG_TEST;Trusted_Connection=true;TrustServerCertificate=True", new SqlServerVectorStoreOptions
    {
        EmbeddingGenerator = embeddingGenerator
    });

SqlServerCollection<Guid, MovieVectorStoreRecord> collection = vectorStore.GetCollection<Guid, MovieVectorStoreRecord>("movies");

bool importData = false;
if (!await collection.CollectionExistsAsync())
{
    importData = true;
}
else
{
    Console.WriteLine("Re-import data?");
    ConsoleKeyInfo key = Console.ReadKey();
    if (key.Key == ConsoleKey.Y)
    {
        importData = true;
    }
}

ChatMessage question = new(ChatRole.User, "What is the 3 highest rated adventure movies?");

await Option1RephraseQuestion.Run(importData, movieDataForRag, question, client, collection, configuration);
await Option2EnhanceEmbeddings.Run(importData, movieDataForRag, question, client, collection, configuration);
await Option3CommonSense.Run(importData, movieDataForRag, question, client, collection, configuration);