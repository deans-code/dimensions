namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when unable to establish a connection to the embedding service.
/// </summary>
public sealed class EmbeddingServiceConnectionException : Exception
{
    public string BaseUrl { get; }

    public EmbeddingServiceConnectionException(string baseUrl, Exception innerException)
        : base($"Cannot connect to embedding service at {baseUrl}. " +
               "Please ensure the embedding model is running.", innerException)
    {
        BaseUrl = baseUrl;
    }
}
