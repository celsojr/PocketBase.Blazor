using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Auth;
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
                ["identity"] = email,
                ["password"] = password
            };

            var result = await _http.SendAsync<AuthResponse>(HttpMethod.Post, "api/collections/_superusers/auth-with-password", body, cancellationToken: cancellationToken);

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
        public async Task<Result<AuthResponse>> AuthRefreshAsync(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new CommonOptions();
            options.Query = options.BuildQuery();

            var result = await _http.SendAsync<AuthResponse>(HttpMethod.Post, "api/_superusers/auth-refresh", query: options.Query, cancellationToken: cancellationToken);

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
        public async Task<Result<AuthResponse>> ImpersonateAsync(string collectionName, string recordId, int duration, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(collectionName, nameof(collectionName));
            ArgumentException.ThrowIfNullOrWhiteSpace(recordId, nameof(recordId));

            var body = new Dictionary<string, object>()
            {
                ["duration"] = duration,
            };

            options ??= new CommonOptions();
            options.Query = options.BuildQuery();

            var result = await _http.SendAsync<AuthResponse>(HttpMethod.Post, $"api/collections/{collectionName}/impersonate/{recordId}", body, options.Query, cancellationToken: cancellationToken);

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
        public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
        {
            await _http.SendAsync(HttpMethod.Post, "api/_superusers/logout", body: null, cancellationToken: cancellationToken);
            _authStore?.Clear();
            return Result.Ok();
        }

        /// <inheritdoc />
        public void SetStore(PocketBaseStore store)
        {
            _authStore = store ?? throw new ArgumentNullException(nameof(store));
        }
    }
}
