using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Enums;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Requests.Batch;
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
            ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
            _transport = transport;
            _collectionName = collectionName;
            _requests = requests;
        }

        /// <inheritdoc />
        public IBatchClient Collection(string collectionName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
            return new BatchClient(_transport, collectionName, _requests);
        }

        /// <inheritdoc />
        public IBatchClient Create(object body, List<BatchFile>? files = null)
        {
            ValidateCollectionName();
            _requests.Add(new BatchRequest(_collectionName!, BatchMethod.Create, body, files: files));
            return this;
        }


        /// <inheritdoc />
        public IBatchClient Delete(string id)
        {
            ValidateCollectionName();
            _requests.Add(new BatchRequest(_collectionName!, BatchMethod.Delete, null, id));
            return this;
        }

        /// <inheritdoc />
        public async Task<Result<List<BatchResponse>>> SendAsync(CancellationToken cancellationToken = default)
        {
            var requests = BuildBatchRequest();
            var body = BuildRequestBody(requests);
    
            return await _transport.SendAsync<List<BatchResponse>>(HttpMethod.Post, "api/batch", body, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public IBatchClient Update(string id, object body, List<BatchFile>? files = null)
        {
            ValidateCollectionName();
            _requests.Add(new BatchRequest(_collectionName!, BatchMethod.Update, body, id, files));
            return this;
        }

        /// <inheritdoc />
        public IBatchClient Upsert(object body)
        {
            ValidateCollectionName();
            _requests.Add(new BatchRequest(_collectionName!, BatchMethod.Upsert, body));
            return this;
        }

        private static object BuildRequestBody(IReadOnlyList<BatchRequest> requests)
        {
            var hasFiles = requests.Any(static r => r.Files?.Count > 0);

            if (!hasFiles)
            {
                return CreatePayload(requests);
            }

            return BuildMultipartFormData(requests);
        }

        private static MultipartFormDataContent BuildMultipartFormData(IReadOnlyList<BatchRequest> requests)
        {
            var form = new MultipartFormDataContent();
            var payload = CreatePayload(requests);
            var payloadJson = JsonSerializer.Serialize(payload);
    
            form.Add(new StringContent(payloadJson, Encoding.UTF8, "application/json"), "@jsonPayload");
            AddFilesToForm(form, requests);
    
            return form;
        }

        private static object CreatePayload(IReadOnlyList<BatchRequest> requests)
        {
            var payload = new
            {
                requests = requests.Select(r => new
                {
                    method = r.Method,
                    url = r.Url,
                    body = r.Body
                })
            };
            return payload;
        }

        private static void AddFilesToForm(MultipartFormDataContent form, IReadOnlyList<BatchRequest> requests)
        {
            for (var i = 0; i < requests.Count; i++)
            {
                var req = requests[i];
                if (req.Files is null) continue;
        
                foreach (var file in req.Files)
                {
                    var content = new StreamContent(file.Content);
            
                    if (!string.IsNullOrWhiteSpace(file.ContentType))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    }
            
                    form.Add(content, $"requests.{i}.{file.Field}", file.FileName);
                }
            }
        }

        private List<BatchRequest> BuildBatchRequest()
        {
            if (_requests.Count == 0)
                throw new InvalidOperationException("No batch requests were added.");

            return _requests;
        }

        private void ValidateCollectionName()
        {
            if (string.IsNullOrWhiteSpace(_collectionName))
                throw new InvalidOperationException("Collection name must be specified before adding requests. Call the 'Collection' method first.");
        }
    }
}

