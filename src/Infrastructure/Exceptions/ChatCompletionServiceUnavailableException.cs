namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when the chat completion service is not available or returns an unsuccessful status code.
/// </summary>
public sealed class ChatCompletionServiceUnavailableException : Exception
{
    public string BaseUrl { get; }
    public int StatusCode { get; }

    public ChatCompletionServiceUnavailableException(string baseUrl, int statusCode)
        : base($"Chat completion service is not available at {baseUrl}. " +
               $"Status code: {statusCode}. " +
               "Please ensure the chat completion service is running.")
    {
        BaseUrl = baseUrl;
        StatusCode = statusCode;
    }
}
