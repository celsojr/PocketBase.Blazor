namespace PocketBase.Blazor.IntegrationTests.Helpers.MailHog;

public class MailHogOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8027";
    public string MessagesEndpoint => $"{BaseUrl}/api/v2/messages";
    public string DeleteEndpoint => $"{BaseUrl}/api/v1/messages";
}
