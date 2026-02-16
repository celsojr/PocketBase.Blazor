namespace PocketBase.Blazor.IntegrationTests.Helpers.MailHog;

using System.Text.Json.Serialization;

public sealed class MailHogMessage
{
    [JsonPropertyName("Content")]
    public MailHogContent? Content { get; init; }

    [JsonPropertyName("Created")]
    public DateTime? Created { get; init; }

    [JsonPropertyName("ID")]
    public string? Id { get; init; }
}
