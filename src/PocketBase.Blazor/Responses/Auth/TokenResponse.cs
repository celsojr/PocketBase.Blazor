using System.Text.Json;

namespace PocketBase.Blazor.Responses.Auth
{
    public sealed class TokenResponse
    {
        public string? Token { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

