using System;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Store
{
    public class PocketBaseStore
    {
        public AuthStore Auth { get; }
        public IRealtimeClient Realtime { get; }

        private AuthResponse? _currentSession;

        public AuthResponse? CurrentSession => _currentSession;

        public PocketBaseStore(AuthStore auth, IRealtimeClient realtime)
        {
             Auth = auth ?? throw new ArgumentNullException(nameof(auth));
            Realtime = realtime ?? throw new ArgumentNullException(nameof(realtime));
        }

        public async Task<AuthResponse> AuthWithPasswordAsync(string identity, string password, CancellationToken cancellationToken = default)
        {
            _currentSession = await Auth.AuthWithPasswordAsync(identity, password, cancellationToken);
            return _currentSession;
        }

        public async Task<AuthResponse> RefreshAsync(CancellationToken cancellationToken = default)
        {
            _currentSession = await Auth.RefreshAsync(cancellationToken);
            return _currentSession;
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            await Auth.LogoutAsync(cancellationToken);
            _currentSession = null;
        }

        public string? Token => _currentSession?.Token;
    }
}
