using System.Text.Json;

namespace PocketBase.Blazor.Responses.Auth
{
    public class UserResponse
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

