using Dimensions.Domain;
using Dimensions.Infrastructure;

namespace Dimensions.Business;

public sealed class Search : IDisposable
{
    private readonly EmbeddingGeneration _embeddingGeneration;
    private readonly VectorStorage _vectorStorage;
    private bool _disposed = false;

    public Search(
        EmbeddingGeneration embeddingGeneration,
        VectorStorage vectorStorage)
    {
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

    ~Search()
    {
        Dispose(false);
    }

    public async Task<List<SearchResult>> FindMatchesAsync(string queryText)
    {
        AugmentedEmbedding augmentedEmbedding = await _embeddingGeneration.GetEmbeddingAsync(queryText);

        return await _vectorStorage.SearchAsync(augmentedEmbedding, 5);
    }
}
