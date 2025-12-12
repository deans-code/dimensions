namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when the chat completion generation request fails with an unsuccessful status code.
/// </summary>
public sealed class ChatCompletionGenerationFailedException : Exception
{
    public int StatusCode { get; }

    public ChatCompletionGenerationFailedException(int statusCode)
        : base($"Failed to generate chat completion. Status code: {statusCode}. " +
               "Please ensure the chat completion service is running.")
    {
        StatusCode = statusCode;
    }
}
