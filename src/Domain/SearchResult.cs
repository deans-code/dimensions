namespace Dimensions.Domain;

public sealed class SearchResult
{
    public float Scored { get; set; }
    
    public string Text { get; set; } = string.Empty;

    public string DocumentTitle { get; set; } = string.Empty;

    public string DocumentId { get; set; } = string.Empty;
}