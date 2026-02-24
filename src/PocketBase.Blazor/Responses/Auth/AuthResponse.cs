using System.Text.Json;

namespace PocketBase.Blazor.Responses.Auth
{
    public class AuthResponse
    {
        public string Token { get; init; } = default!;
        public UserResponse? Record { get; init; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
