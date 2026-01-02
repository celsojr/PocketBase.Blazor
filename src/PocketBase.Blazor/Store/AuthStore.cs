using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Store
{
    public class AuthStore
    {
        public string? Token { get; private set; }
        public UserResponse? User { get; private set; }

        private readonly IAdminsClient _admins;
        private AuthResponse? _currentSession;

        public AuthResponse? CurrentSession => _currentSession;

        public AuthStore(IAdminsClient adminsClient)
        {
            _admins = adminsClient ?? throw new ArgumentNullException(nameof(adminsClient));
        }

        public async Task<Result<AuthResponse>> AuthWithPasswordAsync(string identity, string password, bool isAdmin = false, CancellationToken cancellationToken = default)
        {
            var result = await _admins.AuthWithPasswordAsync(identity, password, isAdmin, cancellationToken: cancellationToken);
            if (result.IsSuccess)
            {
                _currentSession = result.Value;
            }
            return result;
        }

        public async Task<Result<AuthResponse>> RefreshAsync(bool isAdmin = false, CancellationToken cancellationToken = default)
        {
            var result = await _admins.RefreshAsync(isAdmin, cancellationToken: cancellationToken);
            if (result.IsSuccess)
            {
                _currentSession = result.Value;
            }
            return result;
        }

        public async Task<Result> LogoutAsync(bool isAdmin = false, CancellationToken cancellationToken = default)
        {
            await _admins.LogoutAsync(isAdmin, cancellationToken: cancellationToken);
            _currentSession = null;
            return Result.Ok();
        }

        public void Save(AuthResponse auth)
        {
            Token = auth.Token;
            User = auth.Record;
        }

        public void Clear()
        {
            Token = null;
            User = null;
        }
    }
}

