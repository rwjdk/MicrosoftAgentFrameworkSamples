using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using BuildYourOwnVectorStore.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

#pragma warning disable MEVD9001

namespace BuildYourOwnVectorStore.VectorStoreImplementation;

public class MyVectorStoreCollection<TKey, TRecord> : VectorStoreCollection<TKey, TRecord>
    where TRecord : class where TKey : notnull
{
    private readonly PropertyInfo _recordKeyProperty;
    private readonly PropertyInfo _recordVectorProperty;
    private readonly IEnumerable<PropertyInfo> _recordAdditionalProperties;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public MyVectorStoreCollection(string name, VectorStoreCollectionDefinition vectorStoreCollectionDefinition)
    {
        Name = name;
        _embeddingGenerator = (IEmbeddingGenerator<string, Embedding<float>>)vectorStoreCollectionDefinition.EmbeddingGenerator!;
        PropertyInfo[] properties = typeof(TRecord).GetProperties();
        _recordKeyProperty = properties.First(x => x.GetCustomAttribute<VectorStoreKeyAttribute>(inherit: true) is not null);
        _recordVectorProperty = properties.First(x => x.GetCustomAttribute<VectorStoreVectorAttribute>(inherit: true) is not null);
        _recordAdditionalProperties = properties.Where(x => x.GetCustomAttribute<VectorStoreDataAttribute>(inherit: true) is not null);
    }

    public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public override Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        FolderHelper.GetStoreFolder(Name);
        return Task.CompletedTask;
    }

    public override Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        string storeFolder = FolderHelper.GetStoreFolder(Name);
        Directory.Delete(storeFolder, true);
        return Task.CompletedTask;
    }

    public override Task DeleteAsync(TKey key, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public override async Task UpsertAsync(TRecord record, CancellationToken cancellationToken = new CancellationToken())
    {
        await UpsertAsync([record], cancellationToken);
    }

    public override async Task UpsertAsync(IEnumerable<TRecord> records, CancellationToken cancellationToken = new CancellationToken())
    {
        string storeFolder = FolderHelper.GetStoreFolder(Name);

        foreach (TRecord record in records)
        {
            object keyValue = _recordKeyProperty.GetValue(record)!;
            string vectorValue = _recordVectorProperty.GetValue(record)!.ToString()!;
            Embedding<float> embedding = await _embeddingGenerator.GenerateAsync(vectorValue, cancellationToken: cancellationToken);

            Dictionary<string, object?> toSave = new()
            {
                ["Id"] = keyValue,
                ["Vector"] = embedding.Vector
            };
            foreach (PropertyInfo property in _recordAdditionalProperties)
            {
                toSave[property.Name] = property.GetValue(record);
            }
            string json = JsonSerializer.Serialize(toSave);

            await File.WriteAllTextAsync(Path.Combine(storeFolder, keyValue + ".json"), json, cancellationToken);
        }
    }

    public override async IAsyncEnumerable<VectorSearchResult<TRecord>> SearchAsync<TInput>(TInput searchValue, int top, VectorSearchOptions<TRecord>? options = null, CancellationToken cancellationToken = default)
    {
        string? searchString = searchValue.ToString();
        Embedding<float> searchEmbedding = await _embeddingGenerator!.GenerateAsync(searchString, cancellationToken: cancellationToken);

        string storeFolder = FolderHelper.GetStoreFolder(Name);
        string[] vectorFiles = Directory.GetFiles(storeFolder);
        List<VectorSearchResult<TRecord>> allRecords = [];
        foreach (string vectorFile in vectorFiles)
        {
            string json = await File.ReadAllTextAsync(vectorFile, cancellationToken);
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            TRecord record = Activator.CreateInstance<TRecord>();

            // Map stored "Id" to the property's actual name.
            if (root.TryGetProperty("Id", out JsonElement idElement))
            {
                object? id = idElement.Deserialize(_recordKeyProperty.PropertyType);
                _recordKeyProperty.SetValue(record, id);
            }

            //Rest of VectorData Properties
            foreach (PropertyInfo property in _recordAdditionalProperties)
            {
                if (root.TryGetProperty(property.Name, out JsonElement element))
                {
                    object? value = element.Deserialize(property.PropertyType);
                    property.SetValue(record, value);
                }
            }

            // Get stored "Vector" and find its similarity to the search-vector
            float score = 0;
            if (root.TryGetProperty("Vector", out JsonElement vectorElement))
            {
                float[] values = vectorElement.Deserialize<float[]>()!;
                ReadOnlyMemory<float> vector = values.AsMemory();
                score = VectorMatch.MatchScore(vector, searchEmbedding.Vector);
            }

            VectorSearchResult<TRecord> searchResult = new(record, score);
            allRecords.Add(searchResult);
        }

        foreach (VectorSearchResult<TRecord> result in allRecords
                     .OrderByDescending(x => x.Score)
                     .Take(top))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return result;
        }
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        throw new NotImplementedException();
    }

    public override Task<TRecord?> GetAsync(TKey key, RecordRetrievalOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public override IAsyncEnumerable<TRecord> GetAsync(Expression<Func<TRecord, bool>> filter, int top, FilteredRecordRetrievalOptions<TRecord>? options = null, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public override string Name { get; }
}