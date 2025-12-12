using Dimensions.Domain;
using Dimensions.Infrastructure;

namespace Dimensions.Business;

public sealed class ArchivalData : IDisposable
{    
    private readonly ArchivalDataAccess _archivalDataAccess;
    private readonly ChatCompletionGeneration _chatCompletionGeneration;
    private bool _disposed = false;

    public ArchivalData(
        ArchivalDataAccess archivalDataAccess,
        ChatCompletionGeneration chatCompletionGeneration)    
    {
        _archivalDataAccess = archivalDataAccess;
        _chatCompletionGeneration = chatCompletionGeneration;        
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {            
            _chatCompletionGeneration?.Dispose();            
        }
        
        _disposed = true;        
    }

    ~ArchivalData()
    {
        Dispose(false);
    }

    public async Task CreateArchivalDataFilesAsync(List<string> archivalTopics, string systemPrompt)
    {
        foreach (string archivalTopic in archivalTopics)
        {
            string userMessage = $"Document the game: {archivalTopic}";
            
            AugmentedChatCompletion completion = await _chatCompletionGeneration.GetChatCompletionAsync(userMessage, systemPrompt);
            
            string generatedContent = completion.ChatCompletion?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
            
            if (!string.IsNullOrWhiteSpace(generatedContent))
            {
                await _archivalDataAccess.SaveMarkdownFileAsync(generatedContent, archivalTopic);
            }
        }
    }
}