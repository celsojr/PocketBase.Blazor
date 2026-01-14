namespace PocketBase.Blazor.UnitTests.TestHelpers.Builders;

using System.Text.Json.Serialization;
using Blazor.Responses;

public class PostResponse : RecordResponse
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string? Author { get; set; }
    public string? Category { get; set; }

    [JsonPropertyName("is_published")]
    public bool IsPublished { get; set; }
}

