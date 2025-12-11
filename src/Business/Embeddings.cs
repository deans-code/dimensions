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

        string[] lines = markdownContent.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
        
        var currentChunk = new List<string>();

        foreach (string line in lines)
        {
            if (StartNewChunk(currentChunk, line))
            {
                FinaliseChunk(chunks, currentChunk);

                currentChunk.Clear();
            }

            currentChunk.Add(line);
        }
        
        if (currentChunk.Count > 0)
        {
            FinaliseChunk(chunks, currentChunk);
        }

        return chunks;
    }

    private void FinaliseChunk(List<string> chunks, List<string> currentChunk)
    {
        var finalisedChunk = string.Join(Environment.NewLine, currentChunk).Trim();

        if (!string.IsNullOrWhiteSpace(finalisedChunk))
        {
            chunks.Add(finalisedChunk);
        }
    }

    private bool StartNewChunk(List<string> currentChunk, string line)
        => IsHeading(line) && ChunkHasContent(currentChunk);    

    private bool ChunkHasContent(List<string> currentChunk) => currentChunk.Count > 0;    

    private bool IsHeading(string line) => line.TrimStart().StartsWith("#");    

    public async Task CreateEmbeddingsAsync()
    {        
        await _embeddingGeneration.CheckAvailabilityAsync();

        Dictionary<string, string> data = await _rawDataLoader.LoadAllTxtFilesAsync();

        if (data.Count == 0)
        {
            return;
        }        

        await _vectorStorage.CreateCollectionAsync();

        var documentNames = new List<string>(data.Keys);
        ulong pointId = 1;

        for (int i = 0; i < documentNames.Count; i++)
        {
            string documentName = documentNames[i];

            string value = data[documentName];
            
            List<string> chunks = ChunkMarkdownByHeadings(value);

            foreach (string chunk in chunks)
            {                
                if (IsOnlyHeading(chunk))
                {
                    continue;
                }

                AugmentedEmbedding augmentedEmbedding = await _embeddingGeneration.GetEmbeddingAsync(chunk);

                await _vectorStorage.StoreVectorsAsync(augmentedEmbedding, pointId);

                pointId++;
            }
        }
    }

    private static bool IsOnlyHeading(string chunk)
    {
        string[] lines = chunk.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);

        return lines.Length <= 1 && lines.Any(l => l.TrimStart().StartsWith("#"));    
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