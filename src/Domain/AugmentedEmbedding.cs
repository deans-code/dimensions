using System.Text.Json.Serialization;

namespace Dimensions.Domain;

public sealed class AugmentedEmbedding
{
    public required string Text { get; set; }

    public required EmbeddingResponse? Embedding { get; set; }

    public string DocumentTitle { get; set; } = string.Empty;

    public string DocumentId { get; set; } = string.Empty;
}

public sealed class EmbeddingResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<EmbeddingData> Data { get; set; } = new();

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = new();
}

public sealed class EmbeddingData
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("embedding")]
    public List<double> Embedding { get; set; } = new();

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public sealed class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}