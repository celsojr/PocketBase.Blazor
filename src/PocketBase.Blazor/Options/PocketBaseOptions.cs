using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketBase.Blazor.Options;

public class PocketBaseOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }

    public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = false
    };
}
