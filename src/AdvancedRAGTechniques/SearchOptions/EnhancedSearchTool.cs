using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using UsingRAGInAgentFramework.Models;

namespace AdvancedRAGTechniques.SearchOptions;

public class EnhancedSearchTool(SqlServerCollection<Guid, MovieVectorStoreRecord> collection)
{
    public async Task<List<string>> SearchVectorStore(string question, string genre)
    {
        List<string> result = [];
        await foreach (VectorSearchResult<MovieVectorStoreRecord> searchResult in collection.SearchAsync(question, 25,
                           new VectorSearchOptions<MovieVectorStoreRecord>
                           {
                               IncludeVectors = false,
                               Filter = record => record.Genre == genre
                           }))
        {
            MovieVectorStoreRecord record = searchResult.Record;
            result.Add(record.GetTitleAndDetails());
        }

        return result;
    }
}