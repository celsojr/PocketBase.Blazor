using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
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

        public async Task<Result<AuthResponse>> AuthWithPasswordAsync(string identity, string password, CancellationToken cancellationToken = default)
        {
            var result = await Auth.AuthWithPasswordAsync(identity, password, cancellationToken);
            if (result.IsSuccess)
            {
                _currentSession = result.Value;
            }
            return result;
        }

        public async Task<Result<AuthResponse>> RefreshAsync(CancellationToken cancellationToken = default)
        {
            var result = await Auth.RefreshAsync(cancellationToken);
            if (result.IsSuccess)
            {
                _currentSession = result.Value;
            }
            return result;
        }

        public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
        {
            await Auth.LogoutAsync(cancellationToken);
            _currentSession = null;
            return Result.Ok();
        }

        public string? Token => _currentSession?.Token;
    }
}
