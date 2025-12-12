namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when a markdown file fails to load.
/// </summary>
public sealed class MarkdownFileLoadException : Exception
{
    public string FilePath { get; }

    public MarkdownFileLoadException(string filePath, Exception innerException)
        : base($"Failed to load markdown file: {filePath}", innerException)
    {
        FilePath = filePath;
    }
}
