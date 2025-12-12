namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when the archival data directory is not found.
/// </summary>
public sealed class ArchivalDataDirectoryNotFoundException : Exception
{
    public string DirectoryPath { get; }

    public ArchivalDataDirectoryNotFoundException(string directoryPath)
        : base($"Archival data directory not found: {directoryPath}")
    {
        DirectoryPath = directoryPath;
    }
}
