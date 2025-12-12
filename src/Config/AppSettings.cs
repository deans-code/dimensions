namespace Dimensions.Config;

public sealed class AppSettings
{
    public EmbeddingApiSettings EmbeddingApi { get; set; } = new();
    public ChatCompletionApiSettings ChatCompletionApi { get; set; } = new();
    public VectorDatabaseSettings VectorDatabaseApi { get; set; } = new();
    public string DataDirectory { get; set; } = string.Empty;
    public string SystemPromptPath { get; set; } = string.Empty;
    public List<string> ArchivalTopics { get; set; } = new();
}

public sealed class EmbeddingApiSettings
{
    public string Protocol { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Model { get; set; } = string.Empty;
}

public sealed class ChatCompletionApiSettings
{
    public string Protocol { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Model { get; set; } = string.Empty;
}

public sealed class VectorDatabaseSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string CollectionName { get; set; } = string.Empty;
}