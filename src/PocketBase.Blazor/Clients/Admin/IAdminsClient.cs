using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Auth;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor.Clients.Admin
{
    /// <summary>
    /// Provides admin authentication and session management methods.
    /// </summary>
    public interface IAdminsClient
    {
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
        Task<Result<AuthResponse>> AuthRefreshAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Impersonate allows you to authenticate as a different user by generating a nonrefreshable auth token. Only superusers can perform this action.
        /// </summary>
        /// <param name="recordId">ID of the auth record to impersonate.</param>
        /// <param name="duration">Optional custom JWT duration for the exp claim (in seconds). If not set or 0, it fallbacks to the default collection auth token duration option.</param>
        /// <param name="options">Optional common request options such as Fields and Expand</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> with
        /// the outcome of the authentication attempt, including the authentication response if successful.</returns>
        Task<Result<UserResponse>> ImpersonateAsync(string recordId, int duration, CommonOptions? options = null, CancellationToken cancellationToken = default);

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
