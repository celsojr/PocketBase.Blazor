using PocketBase.Blazor.Responses.Auth;

namespace PocketBase.Blazor.Store
{
    /// <summary>
    /// Stores the current authentication state.
    /// </summary>
    public class AuthStore
    {
        /// <summary>
        /// Gets the current JWT token.
        /// </summary>
        public string? Token { get; private set; }

        /// <summary>
        /// Gets the current authentication session.
        /// </summary>
        public AuthResponse? CurrentSession { get; private set; }

        /// <summary>
        /// Saves the authentication session.
        /// </summary>
        /// <param name="auth">The authentication response to save.</param>
        public void Save(AuthResponse auth)
        {
            Token = auth.Token;
            CurrentSession = auth;
        }

        /// <summary>
        /// Clears the current authentication session.
        /// </summary>
        public void Clear()
        {
            Token = null;
            CurrentSession = null;
        }
    }
}
