namespace PocketBase.Blazor.IntegrationTests.Helpers;

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

    /// <summary>
    /// Gets the expanded category if it exists in the Expand dictionary.
    /// </summary>
    [JsonIgnore]
    public CategoryResponse? ExpandedCategory => GetExpandedRecord<CategoryResponse>("category");

    /// <inheritdoc />
    public override string ToString()
    {
        return JsonSerializer.Serialize(this,
            new JsonSerializerOptions { WriteIndented = true });
    }
}

public class CategoryResponse : RecordResponse
{
    public string? Name { get; init; }
    public string? Slug { get; init; }
}

