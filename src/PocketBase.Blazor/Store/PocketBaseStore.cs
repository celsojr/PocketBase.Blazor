using System;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Responses.Auth;

namespace PocketBase.Blazor.Store
{
    public class PocketBaseStore
    {
        public AuthStore Auth { get; }
        public IRealtimeClient Realtime { get; }
        public IRealtimeStreamClient RealtimeSse { get; }

        private AuthResponse? _currentSession;

        public string? Token => _currentSession?.Token;
        public AuthResponse? CurrentSession => _currentSession;

        public PocketBaseStore(AuthStore auth, IRealtimeClient realtime, IRealtimeStreamClient realtimeSse)
        {
            Auth = auth ?? throw new ArgumentNullException(nameof(auth));
            Realtime = realtime ?? throw new ArgumentNullException(nameof(realtime));
            RealtimeSse = realtimeSse ?? throw new ArgumentNullException(nameof(realtimeSse));
        }

        public void Save(AuthResponse auth)
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

