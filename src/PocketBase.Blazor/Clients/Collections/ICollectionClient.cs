using System;
using System.IO;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Requests;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Collections
{
    public interface ICollectionClient
    {
        string Name { get; }

        // Auth (for auth-enabled collections)
        Task<AuthResponse> AuthWithPasswordAsync(string identity, string password);
        Task<AuthResponse> RefreshAsync();
        Task RequestPasswordResetAsync(string email);

        // CRUD
        Task<RecordModel> GetOneAsync(string id, QueryOptionsRequest? options = null);
        Task<ListResult<RecordModel>> GetListAsync(int page = 1, int perPage = 30, QueryOptionsRequest? options = null);
        Task<RecordModel> GetFirstAsync(string filter, QueryOptionsRequest? options = null);

        Task<RecordModel> CreateAsync(object data);
        Task<RecordModel> UpdateAsync(string id, object data);
        Task DeleteAsync(string id);

        // Files
        Task<RecordModel> UploadFileAsync(string id, string field, Stream file, string fileName);

        // Realtime
        Task<IDisposable> SubscribeAsync(string topic, Action<RecordSubscriptionEvent> handler);
    }

}
