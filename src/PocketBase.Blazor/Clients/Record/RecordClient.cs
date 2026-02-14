using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Requests.Auth;
using PocketBase.Blazor.Responses.Auth;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor.Clients.Record
{
    /// <inheritdoc />
    public class RecordClient : BaseClient, IRecordClient
    {
        /// <inheritdoc />
        public string CollectionName { get; }

        /// <inheritdoc />
        protected override string BasePath => $"api/collections/{CollectionName}/records";

        private PocketBaseStore? _authStore;

        /// <inheritdoc />
        public IRealtimeClient Realtime => _authStore?.Realtime
            ?? throw new InvalidOperationException("Realtime client is not available because the auth store is not set.");

        /// <inheritdoc />
        public IRealtimeStreamClient RealtimeSse => _authStore?.RealtimeSse
            ?? throw new InvalidOperationException("Realtime stream client is not available because the auth store is not set.");

        /// <inheritdoc />
        public RecordClient(string collectionName, IHttpTransport http, PocketBaseStore store)
            : base(http)
        {
            CollectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            _authStore = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <inheritdoc />
        public Task<Result<AuthMethodsResponse>> ListAuthMethodsAsync(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Result<AuthResponse>> AuthWithPasswordAsync(string email, string password, string? identityField = null, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must be provided.", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must be provided.", nameof(password));

            var body = new Dictionary<string, object?>
            {
                ["identity"] = email,
                ["password"] = password
            };

            if (!string.IsNullOrWhiteSpace(identityField))
            {
                body["identityField"] = identityField;
            }

            options ??= new CommonOptions();
            options.Query = options.BuildQuery();

            var result = await Http.SendAsync<AuthResponse>(HttpMethod.Post, "api/collections/users/auth-with-password", body, options.Query, cancellationToken: cancellationToken);

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
        public Task<Result<AuthRecordResponse>> AuthWithOAuth2CodeAsync(AuthWithOAuth2Request request, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Result<RequestOtpResponse>> RequestOtpAsync(string email, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
            var body = new Dictionary<string, object> { ["email"] = email };
            return await Http.SendAsync<RequestOtpResponse>(HttpMethod.Post, $"api/collections/{CollectionName}/request-otp", body, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result<AuthResponse>> AuthWithOtpAsync(string otpId, string otpCode, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(otpId, nameof(otpId));
            ArgumentException.ThrowIfNullOrWhiteSpace(otpCode, nameof(otpCode));

            options = options ?? new CommonOptions();
            options.Body = new Dictionary<string, object>
            {
                ["otpId"] = otpId,
                ["password"] = otpCode,
            };

            var result = await Http.SendAsync<AuthResponse>(HttpMethod.Post, $"api/collections/{CollectionName}/auth-with-otp", options.Body, options.ToDictionary(), cancellationToken: cancellationToken);

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
            options = options ?? new CommonOptions();
            options.Query = options.BuildQuery();

            var result = await Http.SendAsync<AuthResponse>(HttpMethod.Post, "api/collections/users/auth-refresh", query: options.Query, cancellationToken: cancellationToken);

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
        public Task<Result> RequestVerificationAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Result> ConfirmVerificationAsync(string token, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Result> ConfirmPasswordResetAsync(string token, string password, string passwordConfirm, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Result> RequestEmailChangeAsync(string newEmail, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Result> ConfirmEmailChangeAsync(string token, string password, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
        {
            await Http.SendAsync(HttpMethod.Post, "api/collections/users/logout", body: null, cancellationToken: cancellationToken);
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
