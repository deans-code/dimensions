namespace Dimensions.Interface.Exceptions;

/// <summary>
/// Exception thrown when archival data files creation fails.
/// </summary>
public sealed class ArchivalDataFilesCreationException : Exception
{
    public ArchivalDataFilesCreationException(string message, Exception innerException)
        : base($"Failed to create archival data files: {message}", innerException)
    {
    }

    public ArchivalDataFilesCreationException(string message)
        : base($"Failed to create archival data files: {message}")
    {
    }
}
