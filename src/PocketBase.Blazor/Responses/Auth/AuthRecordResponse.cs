namespace PocketBase.Blazor.Responses.Auth
{
    public class AuthRecordResponse
    {
        public string? Token { get; init; }
        public UserResponse? Record { get; init; }
        public AuthMetaResponse? Meta { get; init; }
    }
}
