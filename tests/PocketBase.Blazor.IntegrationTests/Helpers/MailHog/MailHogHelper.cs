namespace PocketBase.Blazor.IntegrationTests.Helpers.MailHog;

using System.Text.RegularExpressions;
using static VerificationType;

public class MailHogService : IMailHogService
{
    private readonly HttpClient _httpClient;
    private readonly MailHogOptions _options;

    public MailHogService() : this(new HttpClient(), new MailHogOptions())
    {
    }

    public MailHogService(HttpClient httpClient, MailHogOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<string?> GetLatestOtpCodeAsync(string recipientEmail, int otpLength = 8)
    {
        ArgumentException.ThrowIfNullOrEmpty(recipientEmail);

        MailHogMessage? message = await GetLatestMessageForRecipientAsync(recipientEmail);
        if (message?.Content?.Body == null)
            return null;

        string otpPattern = $@"\b\d{{{otpLength}}}\b";
        return ExtractPattern(message.Content.Body, otpPattern);
    }

    public async Task<string?> GetLatestTokenAsync(string recipientEmail, VerificationType type)
    {
        ArgumentException.ThrowIfNullOrEmpty(recipientEmail);

        MailHogMessage? message = await GetLatestMessageForRecipientAsync(recipientEmail);
        if (message?.Content?.Body == null)
            return null;

        string cleanedBody = CleanQuotedPrintableBody(message.Content.Body);

        // Determine token URL fragment based on type
        string tokenPattern = type switch
        {
            PasswordReset => "password-reset",
            EmailChange => "email-change",
            EmailVerification => "verification",
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported verification type")
        };

        return ExtractPattern(cleanedBody, $@"confirm-{tokenPattern}/([a-zA-Z0-9\-_=]+(?:\.[a-zA-Z0-9\-_=]+)*)", 1);
    }

    public async Task ClearAllMessagesAsync()
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(_options.DeleteEndpoint);
        response.EnsureSuccessStatusCode();
    }

    private async Task<MailHogMessage?> GetLatestMessageForRecipientAsync(string recipientEmail)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_options.MessagesEndpoint);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        MailHogMessages? messages = JsonSerializer.Deserialize<MailHogMessages>(content);

        return messages?.Items?
            .Where(m => m.Content?.Headers?.To?.Contains(recipientEmail) == true)
            .OrderByDescending(m => m.Created)
            .FirstOrDefault();
    }

    private static string? ExtractPattern(string text, string pattern, int groupIndex = 0)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        Match match = Regex.Match(text, pattern);
        return match.Success ? match.Groups[groupIndex].Value : null;
    }

    private static string CleanQuotedPrintableBody(string body)
    {
        return body
            .Replace("=\r\n", "")
            .Replace("\r\n", "");
    }
}
