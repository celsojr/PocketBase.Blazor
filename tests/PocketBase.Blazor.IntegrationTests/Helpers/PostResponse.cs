namespace PocketBase.Blazor.IntegrationTests.Helpers;

using System.Text.Json.Serialization;
using Blazor.Responses;

public class PostResponse : RecordResponse
{
    public string? Title { get; init; }
    public string? Slug { get; init; }
    public string? Content { get; init; }
    public string? Author { get; init; }

    [JsonPropertyName("is_published")]
    public bool IsPublished { get; init; }
}

