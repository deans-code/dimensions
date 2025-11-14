namespace Dimensions.Infrastructure;

public sealed class RawDataLoading
{
    private readonly string _dataDirectoryPath;

    public RawDataLoading(string dataDirectoryPath)
    {
        _dataDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), dataDirectoryPath);
    }
    
    public async Task<Dictionary<string, string>> LoadAllTxtFilesAsync()
    {
        var result = new Dictionary<string, string>();

        if (!Directory.Exists(_dataDirectoryPath))
        {
            Console.WriteLine($"Data directory not found: {_dataDirectoryPath}");

            return result;
        }

        string[] txtFiles = Directory.GetFiles(_dataDirectoryPath, "*.txt");

        foreach (var filePath in txtFiles)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string content = await File.ReadAllTextAsync(filePath);
                result[fileName] = content;
                
                Console.WriteLine($"Loaded file: {fileName} ({content.Length} characters)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file {filePath}: {ex.Message}");
            }
        }

        Console.WriteLine($"Successfully loaded {result.Count} txt files from {_dataDirectoryPath}");

        return result;
    }        
}