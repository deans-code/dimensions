using Dimensions.Infrastructure.Exceptions;

namespace Dimensions.Infrastructure;

public sealed class ArchivalDataAccess
{
    private readonly string _dataDirectoryPath;

    public ArchivalDataAccess(string dataDirectoryPath)
    {
        _dataDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), dataDirectoryPath);
    }
    
    public async Task<Dictionary<string, string>> LoadAllMarkdownFilesAsync()
    {
        var result = new Dictionary<string, string>();

        if (!Directory.Exists(_dataDirectoryPath))
        {
            throw new ArchivalDataDirectoryNotFoundException(_dataDirectoryPath);
        }

        string[] markdownFiles = Directory.GetFiles(_dataDirectoryPath, "*.md");

        foreach (var filePath in markdownFiles)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string content = await File.ReadAllTextAsync(filePath);
                result[fileName] = content;
            }
            catch (Exception ex)
            {
                throw new MarkdownFileLoadException(filePath, ex);
            }
        }

        return result;
    }

    public async Task SaveMarkdownFileAsync(string content, string fileNameTitle)
    {
        if (!Directory.Exists(_dataDirectoryPath))
        {
            Directory.CreateDirectory(_dataDirectoryPath);
        }

        string filePath = Path.Combine(_dataDirectoryPath, $"{fileNameTitle}.md");

        try
        {
            await File.WriteAllTextAsync(filePath, content);
        }
        catch (Exception ex)
        {
            throw new MarkdownFileSaveException(filePath, ex);
        }
    }
}
