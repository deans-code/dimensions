namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when unable to establish a connection to the chat completion service.
/// </summary>
public sealed class ChatCompletionServiceConnectionException : Exception
{
    public string BaseUrl { get; }

    public ChatCompletionServiceConnectionException(string baseUrl, Exception innerException)
        : base($"Cannot connect to chat completion service at {baseUrl}. " +
               "Please ensure the chat completion service is running.", innerException)
    {
        BaseUrl = baseUrl;
    }
}
