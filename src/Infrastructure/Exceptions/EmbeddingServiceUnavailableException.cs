namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when the embedding service is not available or returns an unsuccessful status code.
/// </summary>
public sealed class EmbeddingServiceUnavailableException : Exception
{
    public string BaseUrl { get; }
    public int StatusCode { get; }

    public EmbeddingServiceUnavailableException(string baseUrl, int statusCode)
        : base($"Embedding service is not available at {baseUrl}. " +
               $"Status code: {statusCode}. " +
               "Please ensure the embedding model is running.")
    {
        BaseUrl = baseUrl;
        StatusCode = statusCode;
    }
}
