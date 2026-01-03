using System;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Store
{
    public class PocketBaseStore
    {
        public AuthStore Auth { get; }
        public IRealtimeClient Realtime { get; }

        private AuthResponse? _currentSession;

        public string? Token => _currentSession?.Token;
        public AuthResponse? CurrentSession => _currentSession;

        public PocketBaseStore(AuthStore auth, IRealtimeClient realtime)
        {
            Auth = auth ?? throw new ArgumentNullException(nameof(auth));
            Realtime = realtime ?? throw new ArgumentNullException(nameof(realtime));
        }

        internal void Save(AuthResponse auth)
        {
            _currentSession = auth;
            Auth.Save(auth);
        }

        public void Clear()
        {
            _currentSession = null;
            Auth.Clear();
        }
    }
}

