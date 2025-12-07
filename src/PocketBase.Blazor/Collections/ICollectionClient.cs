using System;
using System.IO;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Collections
{
    public interface ICollectionClient
    {
        string Name { get; }

        // Auth (for auth-enabled collections)
        Task<AuthResult> AuthWithPasswordAsync(string identity, string password);
        Task<AuthResult> RefreshAsync();
        Task RequestPasswordResetAsync(string email);

        // CRUD
        Task<RecordModel> GetOneAsync(string id, QueryOptions? options = null);
        Task<ListResult<RecordModel>> GetListAsync(int page = 1, int perPage = 30, QueryOptions? options = null);
        Task<RecordModel> GetFirstAsync(string filter, QueryOptions? options = null);

        Task<RecordModel> CreateAsync(object data);
        Task<RecordModel> UpdateAsync(string id, object data);
        Task DeleteAsync(string id);

        // Files
        Task<RecordModel> UploadFileAsync(string id, string field, Stream file, string fileName);

        // Realtime
        Task<IDisposable> SubscribeAsync(string topic, Action<RecordSubscriptionEvent> handler);
    }

}
