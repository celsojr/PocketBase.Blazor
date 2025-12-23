using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Requests;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Collections
{
    public class CollectionClient : ICollectionClient
    {
        public string Name { get; }

        readonly IHttpTransport _http;

        public CollectionClient(string name, IHttpTransport http)
        {
            Name = name;
            _http = http;
        }

        public Task<AuthResponse> AuthWithPasswordAsync(string identity, string password, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> CreateAsync(object data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> GetFirstAsync(string filter, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ListResult<RecordModel>> GetListAsync(int page = 1, int perPage = 30, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> GetOneAsync(string id, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponse> RefreshAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> SubscribeAsync(string topic, Action<RecordSubscriptionEvent> handler, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> UpdateAsync(string id, object data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> UploadFileAsync(string id, string field, Stream file, string fileName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
