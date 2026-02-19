namespace PocketBase.Blazor.IntegrationTests.Helpers.MailHog;

using System.Text.Json.Serialization;

public sealed class MailHogMessages
{
    [JsonPropertyName("items")]
    public List<MailHogMessage>? Items { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; }
}
