using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;
using PocketBase.Blazor.Store;
using static System.Net.WebRequestMethods;

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






















        ///// <inheritdoc />
        //public Task<Result<ListResult<RecordResponse>>> GetListAsync(string collection, int page = 1, int perPage = 30, CommonOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
        //    var query = options?.ToDictionary() ?? new Dictionary<string, object?>();
        //    query["page"] = page.ToString();
        //    query["perPage"] = perPage.ToString();
        //    return Http.SendAsync<ListResult<RecordResponse>>(HttpMethod.Get, $"api/collections/{collection}/records", query: query, cancellationToken: cancellationToken);
        //}

        ///// <inheritdoc />
        //public async Task<Result<List<RecordResponse>>> GetFullListAsync(string collection, CommonOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
        //    var all = new List<RecordResponse>();
        //    int page = 1;
        //    const int perPage = 200;
        //    while (true)
        //    {
        //        var result = await GetListAsync(collection, page, perPage, options, cancellationToken).ConfigureAwait(false);
        //        if (result.Value.Items == null || result.Value.Items.Count == 0) break;
        //        all.AddRange(result.Value.Items);
        //        if (result.Value.Items.Count < perPage) break;
        //        page++;
        //    }
        //    return all;
        //}

        ///// <inheritdoc />
        //public Task<Result<RecordResponse>> GetFirstListItemAsync(string collection, string filter, CommonOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
        //    if (string.IsNullOrWhiteSpace(filter)) throw new ArgumentException("Filter is required.", nameof(filter));
        //    var query = options?.ToDictionary() ?? new Dictionary<string, object?>();
        //    query["filter"] = filter;
        //    return Http.SendAsync<RecordResponse>(HttpMethod.Get, $"api/collections/{collection}/records", query: query, cancellationToken: cancellationToken);
        //}

        ///// <inheritdoc />
        //public Task<Result<RecordResponse>> GetOneAsync(string collection, string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
        //    if (string.IsNullOrWhiteSpace(recordId)) throw new ArgumentException("Record ID is required.", nameof(recordId));
        //    var query = options?.ToDictionary();
        //    return Http.SendAsync<RecordResponse>(HttpMethod.Get, $"api/collections/{collection}/records/{recordId}", query: query, cancellationToken: cancellationToken);
        //}

        ///// <inheritdoc />
        //public Task<Result<RecordResponse>> CreateAsync(string collection, object bodyParams, CommonOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
        //    ArgumentNullException.ThrowIfNull(bodyParams);
        //    var query = options?.ToDictionary();
        //    return Http.SendAsync<RecordResponse>(HttpMethod.Post, $"api/collections/{collection}/records", bodyParams, query, cancellationToken);
        //}

        ///// <inheritdoc />
        //public Task<Result<RecordResponse>> UpdateAsync(string collection, string recordId, object bodyParams, CommonOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
        //    if (string.IsNullOrWhiteSpace(recordId)) throw new ArgumentException("Record ID is required.", nameof(recordId));
        //    ArgumentNullException.ThrowIfNull(bodyParams);
        //    var query = options?.ToDictionary();
        //    return Http.SendAsync<RecordResponse>(HttpMethod.Patch, $"api/collections/{collection}/records/{recordId}", bodyParams, query, cancellationToken);
        //}

        ///// <inheritdoc />
        //public async Task<Result<bool>> DeleteAsync(string collection, string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
        //    if (string.IsNullOrWhiteSpace(recordId)) throw new ArgumentException("Record ID is required.", nameof(recordId));
        //    var query = options?.ToDictionary();
        //    await Http.SendAsync(HttpMethod.Delete, $"api/collections/{collection}/records/{recordId}", query: query, cancellationToken: cancellationToken);
        //    return true;
        //}
    }
}

