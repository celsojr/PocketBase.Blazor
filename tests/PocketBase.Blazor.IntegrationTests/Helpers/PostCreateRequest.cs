namespace PocketBase.Blazor.IntegrationTests.Helpers;

using System.Text.Json.Serialization;

public class PostCreateRequest
{
    public string? Category { get; set; }
    public string? Slug { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Content { get; set; }
    [JsonPropertyName("is_published")] public bool IsPublished { get; set; }
}

