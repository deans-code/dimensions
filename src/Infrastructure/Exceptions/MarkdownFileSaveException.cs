namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when a markdown file fails to save.
/// </summary>
public sealed class MarkdownFileSaveException : Exception
{
    public string FilePath { get; }

    public MarkdownFileSaveException(string filePath, Exception innerException)
        : base($"Failed to save markdown file: {filePath}", innerException)
    {
        FilePath = filePath;
    }
}
