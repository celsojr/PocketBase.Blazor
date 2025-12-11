using System;
using System.IO;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;

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

        public Task<AuthResult> AuthWithPasswordAsync(string identity, string password)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> CreateAsync(object data)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> GetFirstAsync(string filter, QueryOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ListResult<RecordModel>> GetListAsync(int page = 1, int perPage = 30, QueryOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> GetOneAsync(string id, QueryOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResult> RefreshAsync()
        {
            throw new NotImplementedException();
        }

        public Task RequestPasswordResetAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> SubscribeAsync(string topic, Action<RecordSubscriptionEvent> handler)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> UpdateAsync(string id, object data)
        {
            throw new NotImplementedException();
        }

        public Task<RecordModel> UploadFileAsync(string id, string field, Stream file, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
