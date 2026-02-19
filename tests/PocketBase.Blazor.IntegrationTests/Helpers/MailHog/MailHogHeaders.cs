namespace PocketBase.Blazor.IntegrationTests.Helpers.MailHog;

using System.Text.Json.Serialization;

public sealed class MailHogHeaders
{
    [JsonPropertyName("To")]
    public List<string>? To { get; init; }

    [JsonPropertyName("From")]
    public List<string>? From { get; init; }

    [JsonPropertyName("Subject")]
    public List<string>? Subject { get; init; }
}
