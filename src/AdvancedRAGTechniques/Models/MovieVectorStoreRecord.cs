using Microsoft.Extensions.VectorData;

namespace UsingRAGInAgentFramework.Models;

public class MovieVectorStoreRecord
{
    [VectorStoreKey]
    public required Guid Id { get; set; }

    [VectorStoreData]
    public required string Title { get; set; }

    [VectorStoreData]
    public required string Plot { get; set; }

    [VectorStoreData]
    public required decimal Rating { get; set; }

    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineDistance, IndexKind = IndexKind.Flat)]
    public string? Embedding => $"Title: {Title} - Rating: {Rating} - Plot: {Plot} - Genre: {Genre}";

    [VectorStoreData]
    public string? Genre { get; set; }

    public string GetTitleAndDetails()
    {
        return $"Title: {Title} - Rating: {Rating} - Plot: {Plot} - Genre: {Genre}";
    }
}