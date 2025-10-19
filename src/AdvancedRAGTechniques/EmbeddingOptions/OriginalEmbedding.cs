using Microsoft.SemanticKernel.Connectors.SqlServer;
using UsingRAGInAgentFramework.Models;

namespace AdvancedRAGTechniques.EmbeddingOptions;

public static class OriginalEmbedding
{
    public static async Task Embed(SqlServerCollection<Guid, MovieVectorStoreRecord> collection, Movie[] movieDataForRag)
    {
        //Delete and re-create (In real life, you most likely find delta instead)
        await collection.EnsureCollectionDeletedAsync();
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
    }
}