using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Responses;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor.Clients.Admin
{
    /// <inheritdoc />
    public class AdminsClient : IAdminsClient
    {
        private PocketBaseStore? _authStore;
        private readonly IHttpTransport _http;

        /// <inheritdoc />
        public AdminsClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <inheritdoc />
        public async Task<Result<AuthResponse>> AuthWithPasswordAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must be provided.", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must be provided.", nameof(password));

            var body = new Dictionary<string, object>
            {
                { "identity", email },
                { "password", password }
            };

            var result = await _http.SendAsync<AuthResponse>(
                HttpMethod.Post,
                //"api/collections/_superusers/auth-with-password",
                "api/collections/users/auth-with-password",
                body,
                cancellationToken: cancellationToken
            );

            if (result.IsSuccess)
            {
                _authStore?.Save(result.Value);
                return Result.Ok(result.Value);
            }
            else
            {
                return Result.Fail(result.Errors);
            }
        }

        /// <inheritdoc />
        public async Task<Result<AuthResponse>> RefreshAsync(CancellationToken cancellationToken = default)
        {
            return await _http.SendAsync<AuthResponse>(
                HttpMethod.Post,
                "api/admins/auth-refresh",
                body: null,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
        {
            await _http.SendAsync(
                HttpMethod.Post,
                "api/admins/logout",
                body: null,
                cancellationToken: cancellationToken
            );
            return Result.Ok();
        }

        public void SetStore(PocketBaseStore store)
        {
            _authStore = store ?? throw new ArgumentNullException(nameof(store));
        }
    }
}

