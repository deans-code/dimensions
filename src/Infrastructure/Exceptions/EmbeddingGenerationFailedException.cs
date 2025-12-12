namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when the embedding generation request fails with an unsuccessful status code.
/// </summary>
public sealed class EmbeddingGenerationFailedException : Exception
{
    public int StatusCode { get; }

    public EmbeddingGenerationFailedException(int statusCode)
        : base($"Failed to generate embedding. Status code: {statusCode}. " +
               "Please ensure the embedding model is running.")
    {
        StatusCode = statusCode;
    }
}
