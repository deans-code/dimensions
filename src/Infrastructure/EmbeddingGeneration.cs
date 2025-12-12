using System.Text;
using System.Text.Json;
using Dimensions.Domain;
using Dimensions.Infrastructure.Exceptions;

namespace Dimensions.Infrastructure;

public sealed class EmbeddingGeneration : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string? _model;
    private bool _disposed = false;

    public EmbeddingGeneration(
        string protocol,
        string host,
        int port,
        string model)
    {
        _httpClient = new HttpClient();
        _baseUrl = $"{protocol}://{host}:{port}";
        _model = model;
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
            _httpClient?.Dispose();
        }
        
        _disposed = true;        
    }

    ~EmbeddingGeneration()
    {
        Dispose(false);
    }

    public async Task<AugmentedEmbedding> GetEmbeddingAsync(string inputText)
    {
        try
        {
            var requestBody = new Dictionary<string, object?> { ["input"] = inputText };
            
            if (!string.IsNullOrEmpty(_model)) requestBody["model"] = _model;

            string json = JsonSerializer.Serialize(requestBody);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/v1/embeddings", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new EmbeddingGenerationFailedException((int)response.StatusCode);
            }

            string responseJson = await response.Content.ReadAsStringAsync();

            EmbeddingResponse? embedding = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson);

            if (embedding == null)
            {
                throw new EmbeddingDeserializationException();
            }

            Normalise(embedding);

            return new AugmentedEmbedding
            {
                Text = inputText,
                Embedding = embedding,
            };
        }
        catch (HttpRequestException ex)
        {
            throw new EmbeddingServiceConnectionException(_baseUrl, ex);
        }
        catch (Exception ex)
        {
            throw new EmbeddingOperationException(ex.Message, ex);
        }
    }

    private void Normalise(EmbeddingResponse embedding)
    {        
        if (embedding.Data.Count > 0 && embedding.Data[0].Embedding.Count > 0)
        {
            var vector = embedding.Data[0].Embedding;

            // AI generated description of how magnitude is calculated:
            // Calculate L2 norm (magnitude): ||v|| = √(v₁² + v₂² + ... + vₙ²)
            // This is the Euclidean length of the vector - the straight-line distance
            // from the origin to the point represented by the vector in n-dimensional space.
            // For example, in 3D: ||[3,4,0]|| = √(9+16+0) = 5
            double magnitude = Math.Sqrt(vector.Sum(x => x * x));

            // Each value in the embedding vector is divided by the magnitude.
            if (magnitude > 0)
            {
                for (int i = 0; i < vector.Count; i++)
                {
                    vector[i] = vector[i] / magnitude;
                }
            }

            // AI generated description of why normalisation per embedding is done:
            // Normalize each embedding individually (L2/unit-length) when you want cosine/angle-based similarity.
            // Cosine similarity compares directions, not magnitudes; L2-normalising each vector makes 
            // cosine = dot-product of unit vectors and preserves relative directions. 
            // Global (dataset-wide) normalization would distort per-vector directions and is 
            // not used for similarity search.
        }
    }
}