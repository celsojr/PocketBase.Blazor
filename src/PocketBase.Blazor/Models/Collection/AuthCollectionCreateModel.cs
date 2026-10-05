using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Models.Collection
{
    public sealed class AuthCollectionCreateModel : CollectionCreateModel
    {
        public string? CreateRule { get; init; }
        public string? UpdateRule { get; init; }
        public string? DeleteRule { get; init; }

        public AuthAlertOptions? AuthAlert { get; init; }
        public OAuth2Options? OAuth2 { get; init; }
        public PasswordAuthOptions PasswordAuth { get; init; } = new();
        public MfaOptions? Mfa { get; init; }
        public OtpOptions? Otp { get; init; }

        public TokenOptions? AuthToken { get; init; }
        public TokenOptions? PasswordResetToken { get; init; }
        public TokenOptions? EmailChangeToken { get; init; }
        public TokenOptions? VerificationToken { get; init; }
        public TokenOptions? FileToken { get; init; }

        public EmailTemplate? VerificationTemplate { get; init; }
        public EmailTemplate? ResetPasswordTemplate { get; init; }
        public EmailTemplate? ConfirmEmailChangeTemplate { get; init; }
    }
}