namespace PocketBase.Blazor.Responses.Auth
{
    public class AuthMethodsResponse
    {
        public PasswordResponse Password { get; init; }
        public Oauth2Response Oauth2 { get; init; }
        public MfaResponse Mfa { get; init; }
        public OtpResponse Otp { get; init; }
    }

    public class PasswordResponse
    {
        public bool Enabled { get; init; }
        public string[] IdentityFields { get; init; }
    }

    public class Oauth2Response
    {
        public bool Enabled { get; init; }
        public ProviderResponse[] Providers { get; init; }
    }

    public class ProviderResponse
    {
        public string Name { get; init; }
        public string DisplayName { get; init; }
        public string State { get; init; }
        public string AuthURL { get; init; }
        public string CodeVerifier { get; init; }
        public string CodeChallenge { get; init; }
        public string CodeChallengeMethod { get; init; }
    }

    public class MfaResponse
    {
        public bool Enabled { get; init; }
        public int Duration { get; init; }
    }

    public class OtpResponse
    {
        public bool Enabled { get; init; }
        public int Duration { get; init; }
    }
}
