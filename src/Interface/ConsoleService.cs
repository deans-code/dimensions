using Dimensions.Business;
using Microsoft.Extensions.Hosting;

namespace Dimensions.Interface;

public sealed class ConsoleService : BackgroundService
{
    private readonly Embeddings _embeddings;
    private bool _disposed = false;

    public ConsoleService(Embeddings embeddings)
    {
        _embeddings = embeddings;
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
            Console.WriteLine("1. Enter search text");
            Console.WriteLine("2. Create vector database from data files");
            Console.WriteLine("3. Delete vector database");
            Console.WriteLine("4. Exit application");
            Console.Write("Your choice: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await Search();
                    break;
                case "2":
                    await CreateVectorDatabase();
                    break;
                case "3":
                    await DeleteVectorDatabase();
                    break;
                case "4":
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

        string userInput = Console.ReadLine();

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

        var result = await _embeddings.FindMatchesAsync(userInput);

        if (result.Count == 0)
        {
            Console.WriteLine("No matches found.");
            return;
        }

        foreach (var match in result)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Score: {match.Scored:F4} | Text:");
            Console.WriteLine(match.Text);
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
}