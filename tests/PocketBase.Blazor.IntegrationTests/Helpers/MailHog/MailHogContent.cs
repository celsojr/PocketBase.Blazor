namespace PocketBase.Blazor.IntegrationTests.Helpers.MailHog;

using System.Text.Json.Serialization;

public sealed class MailHogContent
{
    [JsonPropertyName("Headers")]
    public MailHogHeaders? Headers { get; init; }

    [JsonPropertyName("Body")]
    public string? Body { get; init; }

    [JsonPropertyName("Subject")]
    public string? Subject { get; init; }
}
