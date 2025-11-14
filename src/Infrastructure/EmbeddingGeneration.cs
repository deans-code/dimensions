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

    public async Task<AugmentedEmbedding?> GetEmbeddingAsync(string inputText)
    {
        try
        {
            object requestBody = new { input = inputText };

            string json = JsonSerializer.Serialize(requestBody);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/v1/embeddings", content);

            if (!response.IsSuccessStatusCode) return null;
            
            string responseJson = await response.Content.ReadAsStringAsync();

            EmbeddingResponse? embedding = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson);

            if (embedding == null) return null;

            return new AugmentedEmbedding
            {
                Text = inputText,
                Embedding = embedding,
            };                        
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting embedding: {ex.Message}");

            return null;
        }
    }
}