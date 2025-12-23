namespace PocketBase.Blazor.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; } = default!;
        public UserResponse? Record { get; set; }
    }
}
