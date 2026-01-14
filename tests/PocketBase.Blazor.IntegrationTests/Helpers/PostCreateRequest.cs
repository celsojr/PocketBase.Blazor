namespace PocketBase.Blazor.IntegrationTests.Helpers;

public class PostCreateRequest
{
    public string? Category { get; set; }
    public string? Slug { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Content { get; set; }
    public bool IsPublished { get; set; }
}

