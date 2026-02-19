using System;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Responses.Auth;

namespace PocketBase.Blazor.Store
{
    /// <summary>
    /// Main store for PocketBase authentication and realtime state.
    /// </summary>
    public class PocketBaseStore
    {
        /// <summary>
        /// Gets the authentication store.
        /// </summary>
        public AuthStore Auth { get; }

        /// <summary>
        /// Gets the realtime client for WebSocket connections.
        /// </summary>
        public IRealtimeClient Realtime { get; }

        /// <summary>
        /// Gets the realtime client for SSE connections.
        /// </summary>
        public IRealtimeStreamClient RealtimeSse { get; }

        private AuthResponse? _currentSession;

        /// <summary>
        /// Gets the current JWT token.
        /// </summary>
        public string? Token => _currentSession?.Token;

        /// <summary>
        /// Gets the current authentication session.
        /// </summary>
        public AuthResponse? CurrentSession => _currentSession;

        /// <summary>
        /// Initializes a new instance of the PocketBaseStore.
        /// </summary>
        /// <param name="auth">The authentication store.</param>
        /// <param name="realtime">The WebSocket realtime client.</param>
        /// <param name="realtimeSse">The SSE realtime client.</param>
        public PocketBaseStore(AuthStore auth, IRealtimeClient realtime, IRealtimeStreamClient realtimeSse)
        {
            Auth = auth ?? throw new ArgumentNullException(nameof(auth));
            Realtime = realtime ?? throw new ArgumentNullException(nameof(realtime));
            RealtimeSse = realtimeSse ?? throw new ArgumentNullException(nameof(realtimeSse));
        }

        /// <summary>
        /// Saves the authentication session.
        /// </summary>
        /// <param name="auth">The authentication response to save.</param>
        public void Save(AuthResponse auth)
        {
            _currentSession = auth;
            Auth.Save(auth);
        }

        /// <summary>
        /// Clears the current authentication session.
        /// </summary>
        public void Clear()
        {
            _currentSession = null;
            Auth.Clear();
        }
    }
}
