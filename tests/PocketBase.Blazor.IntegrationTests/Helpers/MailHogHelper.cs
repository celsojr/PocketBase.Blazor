namespace PocketBase.Blazor.IntegrationTests.Helpers;

using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public class MailHogHelper
{
    public static async Task<string> GetOtpCodeFromMailHog(string recipientEmail)
    {
        using var httpClient = new HttpClient();

        // Get messages from MailHog API
        var response = await httpClient.GetAsync("http://localhost:8027/api/v2/messages");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var messages = JsonSerializer.Deserialize<MailHogMessages>(content);

        // Find the latest message for the recipient
        var message = messages?.Items?
            .FirstOrDefault(m => m.Content?.Headers?.To?.Contains(recipientEmail) == true);

        if (message?.Content?.Body == null)
            return null!;

        // Extract OTP code from email body
        var body = message.Content.Body;
        var otpPattern = @"\b\d{8}\b"; // Assuming 8-digit OTP (configurable)
        var match = Regex.Match(body, otpPattern);

        return match.Success ? match.Value : null!;
    }

    public static async Task ClearMailHogMessages()
    {
        using var httpClient = new HttpClient();
        await httpClient.DeleteAsync("http://localhost:8027/api/v1/messages");
    }

    public static async Task<string> GetVerificationTokenFromEmail(string recipientEmail)
    {
        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync("http://localhost:8027/api/v2/messages");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var messages = JsonSerializer.Deserialize<MailHogMessages>(content);

        var message = messages?.Items?
            .FirstOrDefault(m => m.Content?.Headers?.To?.Contains(recipientEmail) == true);

        if (message?.Content?.Body == null)
            return null!;

        var body = message.Content.Body;
    
        // Remove all =\r\n and \r\n line breaks that are part of quoted-printable encoding
        var cleanedBody = body
            .Replace("=\r\n", "")
            .Replace("\r\n", "");
    
        // Extract JWT token from verification link
        var tokenPattern = @"confirm-verification/([a-zA-Z0-9\-_=]+(?:\.[a-zA-Z0-9\-_=]+)*)";
        var match = Regex.Match(cleanedBody, tokenPattern);
    
        if (match.Success)
        {
            var token = match.Groups[1].Value;
            return token;
        }

        return null!;
    }
}

public sealed class MailHogMessages
{
    [JsonPropertyName("items")]
    public List<MailHogMessage>? Items { get; init; }
}

public sealed class MailHogMessage
{
    [JsonPropertyName("Content")]
    public MailHogContent? Content { get; init; }
}

public sealed class MailHogContent
{
    [JsonPropertyName("Headers")]
    public MailHogHeaders? Headers { get; init; }

    [JsonPropertyName("Body")]
    public string? Body { get; init; }
}

public sealed class MailHogHeaders
{
    [JsonPropertyName("To")]
    public List<string>? To { get; init; }
}
