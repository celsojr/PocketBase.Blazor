using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Responses;
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
        public RecordClient(string collectionName, IHttpTransport http, PocketBaseStore store)
            : base(http)
        {
            CollectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            _authStore = store ?? throw new ArgumentNullException(nameof(store));
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

            var result = await Http.SendAsync<AuthResponse>(
                HttpMethod.Post,
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
            return await Http.SendAsync<AuthResponse>(
                HttpMethod.Post,
                "api/collections/users/auth-refresh",
                body: null,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
        {
            await Http.SendAsync(
                HttpMethod.Post,
                "api/collections/users/logout",
                body: null,
                cancellationToken: cancellationToken
            );
            return Result.Ok();
        }

        /// <inheritdoc />
        public void SetStore(PocketBaseStore store)
        {
            _authStore = store ?? throw new ArgumentNullException(nameof(store));
        }
    }
}

