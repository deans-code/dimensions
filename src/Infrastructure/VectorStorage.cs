using Qdrant.Client;
using Qdrant.Client.Grpc;
using Dimensions.Domain;

namespace Dimensions.Infrastructure;

public sealed class VectorStorage : IDisposable
{
    private readonly QdrantClient _client;
    private readonly string _collectionName;
    private bool _disposed = false;

    public VectorStorage(
        string host,
        int port,
        string collectionName)
    {
        _client = new QdrantClient(host, port);
        _collectionName = collectionName;
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
            _client?.Dispose();
        }
        
        _disposed = true;        
    }

    ~VectorStorage()
    {
        Dispose(false);
    }

    public async Task CreateCollectionAsync()
    {
        try
        {
            IReadOnlyList<string> collectionsNames = await _client.ListCollectionsAsync();

            if (collectionsNames.Any(x => x == _collectionName)) return;

            await _client.CreateCollectionAsync(collectionName: _collectionName, vectorsConfig: new VectorParams
            {
                Size = 768,
                Distance = Distance.Dot
            });
        }
        catch (Grpc.Core.RpcException ex)
        {
            throw new InvalidOperationException(
                "Cannot connect to vector database (Qdrant). " +
                "Please ensure the vector database is running. " +
                $"Details: {ex.Status.Detail}", ex);
        }
    }

    public async Task DeleteCollectionAsync()
    {
        try
        {
            IReadOnlyList<string> collectionsNames = await _client.ListCollectionsAsync();

            if (!collectionsNames.Any(x => x == _collectionName)) return;

            await _client.DeleteCollectionAsync(_collectionName);
        }
        catch (Grpc.Core.RpcException ex)
        {
            throw new InvalidOperationException(
                "Cannot connect to vector database (Qdrant). " +
                "Please ensure the vector database is running. " +
                $"Details: {ex.Status.Detail}", ex);
        }
    }

    public async Task StoreVectorsAsync(AugmentedEmbedding augmentedEmbedding, ulong id)
    {
        try
        {
            float[]? vectors = augmentedEmbedding.Embedding?.Data.SelectMany(x => x.Embedding).Select(d => (float)d).ToArray();

            if (vectors == null) return;

            var request = new PointStruct
            {
                Id = id,
                Vectors = vectors,
                Payload = {
                    ["text"] = augmentedEmbedding.Text,
                }
            };

            var operationInfo = await _client.UpsertAsync(collectionName: _collectionName, points: new List<PointStruct>
            {
                request
            });
        }
        catch (Grpc.Core.RpcException ex)
        {
            throw new InvalidOperationException(
                "Cannot connect to vector database (Qdrant). " +
                "Please ensure the vector database is running. " +
                $"Details: {ex.Status.Detail}", ex);
        }
    }

    public async Task<List<SearchResult>> SearchAsync(AugmentedEmbedding augmentedEmbedding, int resultCount = 5)
    {
        try
        {
            float[]? vectors = augmentedEmbedding.Embedding?.Data.SelectMany(x => x.Embedding).Select(d => (float)d).ToArray();

            if (vectors == null) return new List<SearchResult>();

            IReadOnlyList<ScoredPoint> searchResult = await _client.SearchAsync(
                collectionName: _collectionName,
                vector: vectors,
                limit: (ulong)resultCount);

            return [.. searchResult.Select(x => new SearchResult
            {
                Scored = x.Score,
                Text = x.Payload["text"].ToString()
            })];        
        }
        catch (Grpc.Core.RpcException ex)
        {
            throw new InvalidOperationException(
                "Cannot connect to vector database (Qdrant). " +
                "Please ensure the vector database is running. " +
                $"Details: {ex.Status.Detail}", ex);
        }
    }
}