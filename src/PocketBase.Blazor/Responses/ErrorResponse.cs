namespace PocketBase.Blazor.Responses;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }
}

