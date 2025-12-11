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

    public List<string> ChunkMarkdownByHeadings(string markdownContent)
    {
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            return new List<string>();
        }

        var chunks = new List<string>();
        var lines = markdownContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var currentChunk = new List<string>();

        foreach (var line in lines)
        {
            // Check if line is a heading (starts with #)
            if (line.TrimStart().StartsWith("#"))
            {
                // If we have accumulated content, save it as a chunk
                if (currentChunk.Count > 0)
                {
                    var chunk = string.Join(Environment.NewLine, currentChunk).Trim();
                    if (!string.IsNullOrWhiteSpace(chunk))
                    {
                        chunks.Add(chunk);
                    }
                    currentChunk.Clear();
                }
            }

            currentChunk.Add(line);
        }

        // Add the last chunk if there's any content
        if (currentChunk.Count > 0)
        {
            var chunk = string.Join(Environment.NewLine, currentChunk).Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }
        }

        return chunks;
    }

    public async Task CreateEmbeddingsAsync()
    {
        // Check if the embedding service is available before proceeding
        await _embeddingGeneration.CheckAvailabilityAsync();

        Dictionary<string, string> data = await _rawDataLoader.LoadAllTxtFilesAsync();

        if (data.Count == 0)
        {
            return;
        }        

        await _vectorStorage.CreateCollectionAsync();

        var keys = new List<string>(data.Keys);
        ulong pointId = 1;

        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            string value = data[key];

            // Chunk the markdown content by headings
            List<string> chunks = ChunkMarkdownByHeadings(value);

            // Process each chunk separately
            foreach (var chunk in chunks)
            {
                // Skip chunks that only contain a header (no content after the heading line)
                var lines = chunk.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length <= 1 && lines.Any(l => l.TrimStart().StartsWith("#")))
                {
                    continue;
                }

                AugmentedEmbedding augmentedEmbedding = await _embeddingGeneration.GetEmbeddingAsync(chunk);

                await _vectorStorage.StoreVectorsAsync(augmentedEmbedding, pointId);
                pointId++;
            }
        }
    }
    
    public async Task DeleteEmbeddingsAsync()
    {
        await _vectorStorage.DeleteCollectionAsync();
    }

    public async Task<List<SearchResult>> FindMatchesAsync(string queryText)
    {
        AugmentedEmbedding augmentedEmbedding = await _embeddingGeneration.GetEmbeddingAsync(queryText);

        return await _vectorStorage.SearchAsync(augmentedEmbedding, 5);
    }
}