namespace PocketBase.Blazor.IntegrationTests.Helpers.MailHog;

using System.Text.RegularExpressions;

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

        var message = await GetLatestMessageForRecipientAsync(recipientEmail);
        if (message?.Content?.Body == null)
            return null;

        var otpPattern = $@"\b\d{{{otpLength}}}\b";
        return ExtractPattern(message.Content.Body, otpPattern);
    }

    public async Task<string?> GetLatestVerificationTokenAsync(string recipientEmail)
    {
        ArgumentException.ThrowIfNullOrEmpty(recipientEmail);

        var message = await GetLatestMessageForRecipientAsync(recipientEmail);
        if (message?.Content?.Body == null)
            return null;

        var cleanedBody = CleanQuotedPrintableBody(message.Content.Body);
        var tokenPattern = @"confirm-verification/([a-zA-Z0-9\-_=]+(?:\.[a-zA-Z0-9\-_=]+)*)";
        
        return ExtractPattern(cleanedBody, tokenPattern, 1);
    }

    public async Task ClearAllMessagesAsync()
    {
        var response = await _httpClient.DeleteAsync(_options.MessagesEndpoint);
        response.EnsureSuccessStatusCode();
    }

    private async Task<MailHogMessage?> GetLatestMessageForRecipientAsync(string recipientEmail)
    {
        var response = await _httpClient.GetAsync(_options.MessagesEndpoint);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var messages = JsonSerializer.Deserialize<MailHogMessages>(content);

        return messages?.Items?
            .Where(m => m.Content?.Headers?.To?.Contains(recipientEmail) == true)
            .OrderByDescending(m => m.Created)
            .FirstOrDefault();
    }

    private static string? ExtractPattern(string text, string pattern, int groupIndex = 0)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var match = Regex.Match(text, pattern);
        return match.Success ? match.Groups[groupIndex].Value : null;
    }

    private static string CleanQuotedPrintableBody(string body)
    {
        return body
            .Replace("=\r\n", "")
            .Replace("\r\n", "");
    }
}
