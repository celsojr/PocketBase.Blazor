using PocketBase.Blazor.Responses.Auth;

namespace PocketBase.Blazor.Store
{
    public class AuthStore
    {
        public string? Token { get; private set; }
        public AuthResponse? CurrentSession { get; private set; }

        public void Save(AuthResponse auth)
        {
            Token = auth.Token;
            CurrentSession = auth;
        }

        public void Clear()
        {
            Token = null;
            CurrentSession = null;
        }
    }
}
