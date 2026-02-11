using System.Text.Json;

namespace PocketBase.Blazor.Responses.Auth
{
    public class AuthMetaResponse
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public string? Username { get; init; }
        public string? Email { get; init; }
        public bool IsNew { get; init; }
        public string? AvatarURL { get; init; }
        public JsonElement? RawUser { get; init; }
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }
        public string? Expiry { get; init; }
    }
}
