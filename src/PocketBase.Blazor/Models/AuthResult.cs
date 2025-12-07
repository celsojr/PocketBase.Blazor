namespace PocketBase.Blazor.Models
{
    public class AuthResult
    {
        public string Token { get; set; } = default!;
        public User? Record { get; set; }
    }
}
