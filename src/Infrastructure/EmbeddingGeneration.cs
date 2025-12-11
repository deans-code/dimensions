using System.Text;
using System.Text.Json;
using Dimensions.Domain;

namespace Dimensions.Infrastructure;

public sealed class EmbeddingGeneration : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private bool _disposed = false;

    public EmbeddingGeneration(
        string protocol,
        string host,
        int port)
    {
        _httpClient = new HttpClient();

        _baseUrl = $"{protocol}://{host}:{port}";
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

    public async Task CheckAvailabilityAsync()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}/v1/models");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Embedding service is not available at {_baseUrl}. " +
                    $"Status code: {response.StatusCode}. " +
                    "Please ensure the embedding model is running.");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Cannot connect to embedding service at {_baseUrl}. " +
                "Please ensure the embedding model is running.", ex);
        }
    }

    public async Task<AugmentedEmbedding> GetEmbeddingAsync(string inputText)
    {
        try
        {
            object requestBody = new { input = inputText };

            string json = JsonSerializer.Serialize(requestBody);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/v1/embeddings", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Failed to generate embedding. Status code: {response.StatusCode}. " +
                    "Please ensure the embedding model is running.");
            }
            
            string responseJson = await response.Content.ReadAsStringAsync();

            EmbeddingResponse? embedding = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson);

            if (embedding == null)
            {
                throw new InvalidOperationException("Failed to deserialize embedding response.");
            }

            // Normalize the embedding vector
            if (embedding.Data.Count > 0 && embedding.Data[0].Embedding.Count > 0)
            {
                var vector = embedding.Data[0].Embedding;
                
                // Calculate L2 norm (magnitude)
                double magnitude = Math.Sqrt(vector.Sum(x => x * x));
                
                // Normalize by dividing each component by the magnitude
                if (magnitude > 0)
                {
                    for (int i = 0; i < vector.Count; i++)
                    {
                        vector[i] = vector[i] / magnitude;
                    }
                }
            }

            return new AugmentedEmbedding
            {
                Text = inputText,
                Embedding = embedding,
            };                        
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Cannot connect to embedding service at {_baseUrl}. " +
                "Please ensure the embedding model is running.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error getting embedding: {ex.Message}", ex);
        }
    }
}