using Dimensions.Domain;
using Dimensions.Infrastructure;

namespace Dimensions.Business;

public sealed class Embeddings : IDisposable
{    
    private readonly RawDataLoading _rawDataLoader;
    private readonly EmbeddingGeneration _embeddingGeneration;
    private readonly VectorStorage _vectorStorage;
    private bool _disposed = false;

    public Embeddings(
        RawDataLoading rawDataLoading,
        EmbeddingGeneration embeddingGeneration,
        VectorStorage vectorStorage)    
    {
        _rawDataLoader = rawDataLoading;
        _embeddingGeneration = embeddingGeneration;
        _vectorStorage = vectorStorage;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _embeddingGeneration?.Dispose();
            _vectorStorage?.Dispose();
        }
        
        _disposed = true;        
    }

    ~Embeddings()
    {
        Dispose(false);
    }

    public async Task CreateEmbeddingsAsync()
    {
        Dictionary<string, string> data = await _rawDataLoader.LoadAllTxtFilesAsync();

        if (data.Count == 0)
        {
            return;
        }

        await _vectorStorage.CreateCollectionAsync();

        var keys = new List<string>(data.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            string value = data[key];

            AugmentedEmbedding? augmentedEmbedding = await _embeddingGeneration.GetEmbeddingAsync(value);

            if (augmentedEmbedding == null)
            {
                continue;
            }

            await _vectorStorage.StoreVectorsAsync(augmentedEmbedding, (ulong)i + 1);
        }
    }
    
    public async Task DeleteEmbeddingsAsync()
    {
        await _vectorStorage.DeleteCollectionAsync();
    }

    public async Task<List<SearchResult>> FindMatchesAsync(string queryText)
    {
        AugmentedEmbedding? augmentedEmbedding = await _embeddingGeneration.GetEmbeddingAsync(queryText);

        if (augmentedEmbedding == null) 
        {            
            return new List<SearchResult>();
        }

        return await _vectorStorage.SearchAsync(augmentedEmbedding, 5);
    }
}