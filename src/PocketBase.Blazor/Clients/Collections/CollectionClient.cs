using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Requests;
using PocketBase.Blazor.Responses;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor.Clients.Collections
{
    /// <inheritdoc />
    public class CollectionClient : ICollectionClient
    {
        /// <inheritdoc />
        public string Name { get; }

        private readonly IHttpTransport _http;
        private readonly PocketBaseStore _store;

        /// <inheritdoc />
        public CollectionClient(string name, IHttpTransport http, PocketBaseStore store)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <inheritdoc />
        public Task<AuthResponse> AuthWithPasswordAsync(string identity, string password, CancellationToken cancellationToken = default)
        {
            return _store.AuthWithPasswordAsync(identity, password, cancellationToken);
        }

        /// <inheritdoc />
        public Task<AuthResponse> RefreshAsync(CancellationToken cancellationToken = default)
        {
            return _store.RefreshAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            return _store.LogoutAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<RecordModel> CreateAsync(object data, CancellationToken cancellationToken = default)
        {
            return await _http.SendAsync<RecordModel>(
                HttpMethod.Post,
                $"/api/collections/{Name}/records",
                data,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<RecordModel> GetOneAsync(string id, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default)
        {
            var query = options?.ToQueryDictionary();
            return await _http.SendAsync<RecordModel>(
                HttpMethod.Get,
                $"/api/collections/{Name}/records/{id}",
                query: query,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<ListResult<RecordModel>> GetListAsync(int page = 1, int perPage = 30, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default)
        {
            var query = options?.ToQueryDictionary() ?? new Dictionary<string, string>();
            query["page"] = page.ToString();
            query["perPage"] = perPage.ToString();

            return await _http.SendAsync<ListResult<RecordModel>>(
                HttpMethod.Get,
                $"/api/collections/{Name}/records",
                query: query,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<RecordModel> GetFirstAsync(string filter, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default)
        {
            var query = options?.ToQueryDictionary() ?? new Dictionary<string, string>();
            query["filter"] = filter;
            query["page"] = "1";
            query["perPage"] = "1";

            var list = await _http.SendAsync<ListResult<RecordModel>>(
                HttpMethod.Get,
                $"/api/collections/{Name}/records",
                query: query,
                cancellationToken: cancellationToken
            );

            if (list.Items.Count == 0)
                throw new InvalidOperationException("No records matched the filter.");

            return list.Items[0];
        }

        /// <inheritdoc />
        public async Task<RecordModel> UpdateAsync(string id, object data, CancellationToken cancellationToken = default)
        {
            return await _http.SendAsync<RecordModel>(
                HttpMethod.Patch,
                $"/api/collections/{Name}/records/{id}",
                data,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            await _http.SendAsync(
                HttpMethod.Delete,
                $"/api/collections/{Name}/records/{id}",
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<RecordModel> UploadFileAsync(string id, string field, Stream file, string fileName, CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file), field, fileName);

            return await _http.SendAsync<RecordModel>(
                HttpMethod.Post,
                $"/api/collections/{Name}/records/{id}/files/{field}",
                content,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object> { { "email", email } };
            await _http.SendAsync(
                HttpMethod.Post,
                $"/api/collections/{Name}/request-password-reset",
                body,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public Task<IDisposable> SubscribeAsync(string topic, Action<RecordSubscriptionEvent> handler, CancellationToken cancellationToken = default)
        {
            return _store.Realtime.SubscribeAsync(topic, handler, cancellationToken);
        }
    }
}
