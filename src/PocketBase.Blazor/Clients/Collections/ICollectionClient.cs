using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Requests;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Collections
{
    public interface ICollectionClient
    {
        /// <summary>
        /// The name of the collection.
        /// </summary>
        string Name { get; }

        // Auth (for auth-enabled collections)
        Task<AuthResponse> AuthWithPasswordAsync(string identity, string password, CancellationToken cancellationToken = default);
        Task<AuthResponse> RefreshAsync(CancellationToken cancellationToken = default);
        Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

        // CRUD
        Task<RecordModel> GetOneAsync(string id, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default);
        Task<ListResult<RecordModel>> GetListAsync(int page = 1, int perPage = 30, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default);
        Task<RecordModel> GetFirstAsync(string filter, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default);

        Task<RecordModel> CreateAsync(object data, CancellationToken cancellationToken = default);
        Task<RecordModel> UpdateAsync(string id, object data, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);

        // Files
        Task<RecordModel> UploadFileAsync(string id, string field, Stream file, string fileName, CancellationToken cancellationToken = default);

        // Realtime
        Task<bool> SubscribeAsync(string topic, Action<RealtimeEvent> handler, CancellationToken cancellationToken = default);
    }

}
