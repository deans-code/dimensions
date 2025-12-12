using Dimensions.Domain;
using Dimensions.Infrastructure;

namespace Dimensions.Business;

public sealed class Search : IDisposable
{
    private readonly EmbeddingGeneration _embeddingGeneration;
    private readonly VectorDataAccess _vectorDataAccess;
    private bool _disposed = false;

    public Search(
        EmbeddingGeneration embeddingGeneration,
        VectorDataAccess vectorDataAccess)
    {
        _embeddingGeneration = embeddingGeneration;
        _vectorDataAccess = vectorDataAccess;
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
            _vectorDataAccess?.Dispose();
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

        return await _vectorDataAccess.SearchAsync(augmentedEmbedding, 5);
    }
}
