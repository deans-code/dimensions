namespace Dimensions.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when the chat completion service response cannot be deserialized.
/// </summary>
public sealed class ChatCompletionDeserializationException : Exception
{
    public ChatCompletionDeserializationException()
        : base("Failed to deserialize chat completion response.")
    {
    }
}
