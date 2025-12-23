using System;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Store
{
    public class AuthStore
    {
        private readonly IAdminsClient _admins;
        private AuthResponse? _currentSession;

        public AuthResponse? CurrentSession => _currentSession;
        public string? Token => _currentSession?.Token;

        public AuthStore(IAdminsClient adminsClient)
        {
            _admins = adminsClient ?? throw new ArgumentNullException(nameof(adminsClient));
        }

        public async Task<AuthResponse> AuthWithPasswordAsync(string identity, string password, CancellationToken cancellationToken = default)
        {
            _currentSession = await _admins.AuthWithPasswordAsync(identity, password, cancellationToken);
            return _currentSession;
        }

        public async Task<AuthResponse> RefreshAsync(CancellationToken cancellationToken = default)
        {
            _currentSession = await _admins.RefreshAsync(cancellationToken);
            return _currentSession;
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            await _admins.LogoutAsync(cancellationToken);
            _currentSession = null;
        }
    }
}

