using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Enums;
using PocketBase.Blazor.Requests;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Batch
{
    /// <inheritdoc />
    public class BatchClient : IBatchClient
    {
        private readonly string? _collectionName;
        private readonly List<BatchRequest> _requests = new();

        /// <inheritdoc />
        public BatchClient() { }

        private BatchClient(string? collectionName, List<BatchRequest> requests)
        {
            _collectionName = collectionName;
            _requests = requests;
        }

        /// <inheritdoc />
        public IBatchClient Collection(string collectionName)
        {
            return new BatchClient(collectionName, _requests);
        }

        /// <inheritdoc />
        public IBatchClient Create(object body)
        {
            _requests.Add(new BatchRequest(_collectionName, BatchMethod.Create, body));
            return this;
        }

        /// <inheritdoc />
        public IBatchClient Delete(string id)
        {
            _requests.Add(new BatchRequest(_collectionName, BatchMethod.Delete, null, id));
            return this;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<BatchResponse>> SendAsync(
            CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(_requests);

            using var httpClient = new HttpClient();
            using var response = await httpClient.PostAsync(
                "/api/batch",
                new StringContent(json, Encoding.UTF8, "application/json"),
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<List<BatchResponse>>(
                stream,
                cancellationToken: cancellationToken
            );

            return result ?? [];
        }

        /// <inheritdoc />
        public IBatchClient Update(string id, object body)
        {
            _requests.Add(new BatchRequest(_collectionName, BatchMethod.Update, body, id));
            return this;
        }

        /// <inheritdoc />
        public IBatchClient Upsert(object body)
        {
            _requests.Add(new BatchRequest(_collectionName, BatchMethod.Upsert, body));
            return this;
        }
    }
}

