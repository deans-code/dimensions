namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when unable to connect to the vector database.
/// </summary>
public sealed class VectorDatabaseConnectionException : Exception
{
    public VectorDatabaseConnectionException(string details, Exception innerException)
        : base($"Cannot connect to vector database (Qdrant). " +
               $"Please ensure the vector database is running. " +
               $"Details: {details}", innerException)
    {
    }
}
