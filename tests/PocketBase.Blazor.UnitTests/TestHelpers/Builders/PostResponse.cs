namespace PocketBase.Blazor.UnitTests.TestHelpers.Builders;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blazor.Responses;

public sealed class PostResponse : RecordResponse
{
    public string? Title { get; init; }
    public string? Slug { get; init; }
    public string? Content { get; init; }
    public string? Author { get; init; }
    public string? Category { get; init; }

    [JsonPropertyName("is_published")]
    public bool IsPublished { get; init; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
    }
}

