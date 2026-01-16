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
        if (Expand?.TryGetValue("category", out var categoryExpansion) == true && 
            categoryExpansion != null)
        {
            try
            {
                // Handle single expansion (object) vs multiple expansion (array)
                if (categoryExpansion is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        return JsonSerializer.Deserialize<CategoryResponse>(
                            jsonElement.GetRawText(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
            }
            catch
            {
                // Log if needed, return null if deserialization fails
                return null;
            }
        }
        return null;
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

