using Dimensions.Business;
using Dimensions.Config;
using Dimensions.Interface.Exceptions;
using Microsoft.Extensions.Hosting;

namespace Dimensions.Interface;

public sealed class ConsoleService : BackgroundService
{
    private readonly Embeddings _embeddings;
    private readonly ArchivalData _archivalData;
    private readonly Search _search;
    private readonly AppSettings _appSettings;
    private bool _disposed = false;

    public ConsoleService(
        Embeddings embeddings,
        ArchivalData archivalData,
        Search search,
        AppSettings appSettings)
    {
        _embeddings = embeddings;
        _archivalData = archivalData;
        _search = search;
        _appSettings = appSettings;
    }
    
    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        base.Dispose();
    }
    
    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {            
            _embeddings?.Dispose();
            _archivalData?.Dispose();
            _search?.Dispose();
        }
        
        _disposed = true;        
    }

    ~ConsoleService()
    {
        Dispose(false);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool continueRunning = true;

        while (continueRunning)
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Create archival data files");
            Console.WriteLine("2. Delete vector database");
            Console.WriteLine("3. Create vector database from data files");
            Console.WriteLine("4. Enter search text");
            Console.WriteLine("5. Exit application");
            Console.Write("Your choice: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await CreateArchivalDataFiles();
                    break;
                case "2":
                    await DeleteVectorDatabase();
                    break;
                case "3":
                    await CreateVectorDatabase();
                    break;
                case "4":
                    await Search();
                    break;
                case "5":
                    continueRunning = false;
                    Console.WriteLine("Closing application.");
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            Console.WriteLine();
        }
    }

    private async Task Search()
    {
        Console.WriteLine();
        Console.Write("Enter your search query (or 'back' to return to main menu): ");

        string? userInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userInput))
        {
            Console.WriteLine("No text entered.");
            return;
        }

        if (userInput.ToLower() == "back")
        {
            Console.WriteLine("Returning to main menu...");
            return;
        }

        var result = await _search.FindMatchesAsync(userInput);

        if (result.Count == 0)
        {
            Console.WriteLine("No matches found.");
            return;
        }

        foreach (var match in result)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Score: {match.Scored:F4} | Document: {match.DocumentTitle}");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();
        }
    }

    private async Task CreateVectorDatabase()
    {
        try
        {
            await _embeddings.CreateEmbeddingsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating vector database: {ex.Message}");
        }
    }

    private async Task DeleteVectorDatabase()
    {
        try
        {
            await _embeddings.DeleteEmbeddingsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting vector database: {ex.Message}");
        }
    }

    private async Task CreateArchivalDataFiles()
    {
        try
        {
            string promptPath = Path.Combine(Directory.GetCurrentDirectory(), _appSettings.SystemPromptPath);
            
            if (!File.Exists(promptPath))
            {
                throw new ArchivalDataFilesCreationException($"System prompt file not found: {promptPath}");
            }

            string systemPrompt = await File.ReadAllTextAsync(promptPath);            
            
            await _archivalData.CreateArchivalDataFilesAsync(_appSettings.ArchivalTopics, systemPrompt);                        
        }
        catch (ArchivalDataFilesCreationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ArchivalDataFilesCreationException(ex.Message, ex);
        }
    }
}