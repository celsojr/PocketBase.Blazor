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

    public CategoryResponse? GetExpandedCategory()
    {
        if (Expand?.TryGetValue("category", out var categoryExpansion) != true ||
            categoryExpansion is not JsonElement jsonElement)
        {
            return null;
        }

        if (jsonElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CategoryResponse>(
                jsonElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return null;
        }
    }

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

