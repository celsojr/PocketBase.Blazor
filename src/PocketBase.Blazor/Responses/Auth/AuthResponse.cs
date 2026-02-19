using System.Text.Json;

namespace PocketBase.Blazor.Responses.Auth
{
    public sealed class AuthResponse
    {
        public string Token { get; set; } = default!;
        public UserResponse? Record { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
