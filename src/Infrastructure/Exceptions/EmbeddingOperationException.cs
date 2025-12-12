namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when an unexpected error occurs during embedding operations.
/// </summary>
public sealed class EmbeddingOperationException : Exception
{
    public EmbeddingOperationException(string message, Exception innerException)
        : base($"Error getting embedding: {message}", innerException)
    {
    }
}
