using Dimensions.Domain;
using Dimensions.Infrastructure;

namespace Dimensions.Business;

public sealed class Embeddings : IDisposable
{    
    private readonly ArchivalDataAccess _archivalDataAccess;
    private readonly EmbeddingGeneration _embeddingGeneration;
    private readonly VectorStorage _vectorStorage;
    private readonly List<string> _contextSectionChunkTitles;
    private bool _disposed = false;

    public Embeddings(
        ArchivalDataAccess archivalDataAccess,
        EmbeddingGeneration embeddingGeneration,
        VectorStorage vectorStorage,
        List<string> contextSectionChunkTitles)    
    {
        _archivalDataAccess = archivalDataAccess;
        _embeddingGeneration = embeddingGeneration;
        _vectorStorage = vectorStorage;
        _contextSectionChunkTitles = contextSectionChunkTitles;
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
        Dictionary<string, string> data = await _archivalDataAccess.LoadAllMarkdownFilesAsync();

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
            string documentId = Guid.NewGuid().ToString();

            string value = data[documentName];
            
            List<string> chunks = ChunkMarkdownByHeadings(value);
            
            List<string> contextChunks = ExtractContextChunks(chunks, _contextSectionChunkTitles);

            foreach (string chunk in chunks)
            {                
                if (IsOnlyHeading(chunk))
                {
                    continue;
                }
                
                string contextualizedChunk = AddContextToChunk(chunk, contextChunks);

                AugmentedEmbedding augmentedEmbedding = await _embeddingGeneration.GetEmbeddingAsync(contextualizedChunk);
                augmentedEmbedding.DocumentTitle = documentName;
                augmentedEmbedding.DocumentId = documentId;

                await _vectorStorage.StoreVectorsAsync(augmentedEmbedding, pointId);

                pointId++;
            }
        }
    }

    private bool IsOnlyHeading(string chunk)
    {
        string[] lines = chunk.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);

        return lines.Length <= 1 && lines.Any(l => l.TrimStart().StartsWith("#"));    
    }

    private List<string> ExtractContextChunks(List<string> chunks, List<string> contextTitles)
    {
        var contextChunks = new List<string>();

        foreach (string chunk in chunks)
        {
            foreach (string contextTitle in contextTitles)
            {
                if (ChunkMatchesTitle(chunk, contextTitle))
                {
                    contextChunks.Add(chunk);
                    break;
                }
            }
        }

        return contextChunks;
    }

    private bool ChunkMatchesTitle(string chunk, string title)
    {
        string[] lines = chunk.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length == 0)
        {
            return false;
        }

        string firstLine = lines[0].Trim('#', ' ');
        
        return firstLine.Equals(title, StringComparison.OrdinalIgnoreCase);
    }

    private string AddContextToChunk(string chunk, List<string> contextChunks)
    {
        if (contextChunks.Count == 0)
        {
            return chunk;
        }

        var chunksToAdd = new List<string>();

        foreach (string contextChunk in contextChunks)
        {            
            if (!chunk.Contains(contextChunk, StringComparison.OrdinalIgnoreCase))
            {
                chunksToAdd.Add(contextChunk);
            }
        }

        if (chunksToAdd.Count == 0)
        {
            return chunk;
        }
        
        string context = string.Join(Environment.NewLine + Environment.NewLine, chunksToAdd);
        
        return context + Environment.NewLine + Environment.NewLine + chunk;
    }

    public async Task DeleteEmbeddingsAsync()
    {
        await _vectorStorage.DeleteCollectionAsync();
    }
}