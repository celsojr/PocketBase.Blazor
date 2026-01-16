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

    /// <summary>
    /// Gets the expanded category if it exists in the Expand dictionary.
    /// </summary>
    [JsonIgnore]
    public CategoryResponse? ExpandedCategory => GetExpandedRecord<CategoryResponse>("category");

    /// <summary>
    /// Gets expanded tags if they exist in the Expand dictionary.
    /// (Assuming you have a TagResponse model)
    /// </summary>
    [JsonIgnore]
    public List<TagResponse> ExpandedTags => GetExpandedList<TagResponse>("tags");

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

public class TagResponse : RecordResponse
{
    public string? Name { get; init; }
}

