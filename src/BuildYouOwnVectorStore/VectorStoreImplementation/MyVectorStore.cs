using Microsoft.Extensions.VectorData;

namespace BuildYourOwnVectorStore.VectorStoreImplementation;

public class MyVectorStore(MyVectorStoreOptions options) : VectorStore
{
    public override VectorStoreCollection<TKey, TRecord> GetCollection<TKey, TRecord>(string name, VectorStoreCollectionDefinition? definition = null)
    {
        MyVectorStoreCollection<TKey, TRecord> collection = new(name, definition ?? new VectorStoreCollectionDefinition
        {
            EmbeddingGenerator = options.EmbeddingGenerator
        });
        return collection;
    }
    
    public override VectorStoreCollection<object, Dictionary<string, object?>> GetDynamicCollection(string name, VectorStoreCollectionDefinition definition)
    {
        throw new NotImplementedException();
    }

    public override IAsyncEnumerable<string> ListCollectionNamesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> CollectionExistsAsync(string name, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public override Task EnsureCollectionDeletedAsync(string name, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        throw new NotImplementedException();
    }
}