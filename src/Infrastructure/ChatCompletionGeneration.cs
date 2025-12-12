using System.Text;
using System.Text.Json;
using Dimensions.Domain;
using Dimensions.Infrastructure.Exceptions;

namespace Dimensions.Infrastructure;

public sealed class ChatCompletionGeneration : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string? _model;
    private bool _disposed = false;

    public ChatCompletionGeneration(
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

    ~ChatCompletionGeneration()
    {
        Dispose(false);
    }

    public async Task<AugmentedChatCompletion> GetChatCompletionAsync(string inputText, string? systemPrompt)
    {
        try
        {
            var messagesList = new List<object>();
            
            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                messagesList.Add(new { role = "system", content = systemPrompt });
            }
            
            messagesList.Add(new { role = "user", content = inputText });
            
            var requestBody = new Dictionary<string, object?> 
            { 
                ["messages"] = messagesList.ToArray()
            };
            
            if (!string.IsNullOrEmpty(_model)) requestBody["model"] = _model;

            string json = JsonSerializer.Serialize(requestBody);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ChatCompletionGenerationFailedException((int)response.StatusCode);
            }

            string responseJson = await response.Content.ReadAsStringAsync();

            ChatCompletionResponse? chatCompletion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);

            if (chatCompletion == null)
            {
                throw new ChatCompletionDeserializationException();
            }            

            return new AugmentedChatCompletion
            {
                Text = inputText,
                ChatCompletion = chatCompletion,
            };
        }
        catch (HttpRequestException ex)
        {
            throw new ChatCompletionServiceConnectionException(_baseUrl, ex);
        }
        catch (Exception ex)
        {
            throw new ChatCompletionOperationException(ex.Message, ex);
        }
    }
}
