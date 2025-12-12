namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when the embedding service response cannot be deserialized.
/// </summary>
public sealed class EmbeddingDeserializationException : Exception
{
    public EmbeddingDeserializationException()
        : base("Failed to deserialize embedding response.")
    {
    }
}
