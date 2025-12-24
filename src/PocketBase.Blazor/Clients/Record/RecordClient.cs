using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Record
{
    /// <inheritdoc />
    public class RecordClient : IRecordClient
    {
        private readonly IHttpTransport _http;

        /// <inheritdoc />
        public RecordClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <inheritdoc />
        public Task<ListResult<RecordResponse>> GetListAsync(string collection, int page = 1, int perPage = 30, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
            var query = options?.ToDictionary() ?? new Dictionary<string, string>();
            query["page"] = page.ToString();
            query["perPage"] = perPage.ToString();
            return _http.SendAsync<ListResult<RecordResponse>>(HttpMethod.Get, $"/api/collections/{collection}/records", query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<RecordResponse>> GetFullListAsync(string collection, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
            var all = new List<RecordResponse>();
            int page = 1;
            const int perPage = 200;
            while (true)
            {
                var result = await GetListAsync(collection, page, perPage, options, cancellationToken).ConfigureAwait(false);
                if (result.Items == null || result.Items.Count == 0) break;
                all.AddRange(result.Items);
                if (result.Items.Count < perPage) break;
                page++;
            }
            return all;
        }

        /// <inheritdoc />
        public Task<RecordResponse> GetFirstListItemAsync(string collection, string filter, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
            if (string.IsNullOrWhiteSpace(filter)) throw new ArgumentException("Filter is required.", nameof(filter));
            var query = options?.ToDictionary() ?? new Dictionary<string, string>();
            query["filter"] = filter;
            return _http.SendAsync<RecordResponse>(HttpMethod.Get, $"/api/collections/{collection}/records", query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<RecordResponse> GetOneAsync(string collection, string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
            if (string.IsNullOrWhiteSpace(recordId)) throw new ArgumentException("Record ID is required.", nameof(recordId));
            var query = options?.ToDictionary();
            return _http.SendAsync<RecordResponse>(HttpMethod.Get, $"/api/collections/{collection}/records/{recordId}", query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<RecordResponse> CreateAsync(string collection, object bodyParams, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
            ArgumentNullException.ThrowIfNull(bodyParams);
            var query = options?.ToDictionary();
            return _http.SendAsync<RecordResponse>(HttpMethod.Post, $"/api/collections/{collection}/records", bodyParams, query, cancellationToken);
        }

        /// <inheritdoc />
        public Task<RecordResponse> UpdateAsync(string collection, string recordId, object bodyParams, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
            if (string.IsNullOrWhiteSpace(recordId)) throw new ArgumentException("Record ID is required.", nameof(recordId));
            ArgumentNullException.ThrowIfNull(bodyParams);
            var query = options?.ToDictionary();
            return _http.SendAsync<RecordResponse>(HttpMethod.Patch, $"/api/collections/{collection}/records/{recordId}", bodyParams, query, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string collection, string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collection)) throw new ArgumentException("Collection is required.", nameof(collection));
            if (string.IsNullOrWhiteSpace(recordId)) throw new ArgumentException("Record ID is required.", nameof(recordId));
            var query = options?.ToDictionary();
            await _http.SendAsync(HttpMethod.Delete, $"/api/collections/{collection}/records/{recordId}", query: query, cancellationToken: cancellationToken);
            return true;
        }
    }
}
