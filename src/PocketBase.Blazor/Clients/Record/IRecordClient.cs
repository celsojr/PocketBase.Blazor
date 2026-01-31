using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Responses.Auth;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor.Clients.Record
{
    /// <summary>
    /// Provides access to PocketBase collection record CRUD APIs.
    /// Mirrors the behavior of the JS SDK RecordService.
    /// </summary>
    public interface IRecordClient : IBaseClient
    {
        /// <summary>
        /// Gets the collection name associated with this client.
        /// </summary>
        string CollectionName { get; }

        /// <summary>
        /// Gets the Realtime Client associated with this record client.
        /// </summary>
        IRealtimeClient Realtime { get; }

        /// <summary>
        /// Gets the Realtime Stream Client associated with this record client.
        /// </summary>
        IRealtimeStreamClient RealtimeSse { get; }

        /// <summary>
        /// Authenticates an admin using email and password.
        /// </summary>
        /// <param name="email">Admin email or identity.</param>
        /// <param name="password">Admin password.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The authentication response containing the session token.</returns>
        Task<Result<AuthResponse>> AuthWithPasswordAsync(string email, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the currently authenticated admin session.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated authentication response.</returns>
        Task<Result<AuthResponse>> RefreshAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs out the currently authenticated admin.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<Result> LogoutAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the PocketBase store for managing authentication state.
        /// </summary>
        /// <param name="store">The PocketBase store instance.</param>
        void SetStore(PocketBaseStore store);
    }
}

