namespace PocketBase.Blazor.Requests.Auth
{
    public class AuthWithOAuth2Request
    {
        public required string Provider { get; init; }
        public required string Code { get; init; }
        public required string CodeVerifier { get; init; }
        public required string RedirectUrl { get; init; }
        public object CreateData { get; init; }
    }
}
