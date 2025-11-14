namespace Dimensions.Domain;

public sealed class SearchResult
{
    public float Scored { get; set; }
    
    public string Text { get; set; } = string.Empty;
}