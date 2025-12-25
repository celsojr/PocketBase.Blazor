using System.Text.Json;

namespace PocketBase.Blazor.Options;

public class PocketBaseOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }

    public JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new PocketBaseDateTimeConverter()
        }
    };
}
