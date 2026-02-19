using System.Text.Json;

namespace PocketBase.Blazor.Responses.Auth
{
    public sealed class UserResponse : RecordResponse
    {
        public string Email { get; init; } = null!;
        public bool Verified { get; init; }
        public bool EmailVisibility { get; init; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
