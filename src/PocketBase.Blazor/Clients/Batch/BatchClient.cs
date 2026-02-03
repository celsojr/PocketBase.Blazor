using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Enums;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Requests;
using PocketBase.Blazor.Responses.Backup;

namespace PocketBase.Blazor.Clients.Batch
{
    /// <inheritdoc />
    public class BatchClient : IBatchClient
    {
        private readonly IHttpTransport _transport;
        private readonly string? _collectionName;
        private readonly List<BatchRequest> _requests = [];

        /// <inheritdoc />
        public BatchClient(IHttpTransport transport)
        {
            _transport = transport;
        }

        private BatchClient(IHttpTransport transport, string? collectionName, List<BatchRequest> requests)
        {
            _transport = transport;
            _collectionName = collectionName;
            _requests = requests;
        }

        /// <inheritdoc />
        public IBatchClient Collection(string collectionName)
        {
            return new BatchClient(_transport, collectionName, _requests);
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
        public Task<Result<List<BatchResponse>>> SendAsync(CancellationToken cancellationToken = default)
        {
            return _transport.SendAsync<List<BatchResponse>>(HttpMethod.Post, "api/batch", _requests, cancellationToken: cancellationToken);
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

