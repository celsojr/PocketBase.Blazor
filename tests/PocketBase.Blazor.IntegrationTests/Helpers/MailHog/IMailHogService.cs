namespace PocketBase.Blazor.IntegrationTests.Helpers.MailHog;

public interface IMailHogService
{
    Task<string?> GetLatestOtpCodeAsync(string recipientEmail, int otpLength = 8);
    Task<string?> GetLatestTokenAsync(string recipientEmail, VerificationType type);
    Task ClearAllMessagesAsync();
}
