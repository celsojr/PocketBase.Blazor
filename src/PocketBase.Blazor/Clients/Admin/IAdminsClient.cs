using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Responses;

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
        Task<Result<AuthResponse>> RefreshAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs out the currently authenticated admin.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
    }
}

