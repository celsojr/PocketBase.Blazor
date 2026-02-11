using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Requests.Auth;
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
        /// Retrieves a list of enabled authentication methods for the current Vault namespace.
        /// </summary>
        /// <remarks>This method queries the Vault server for all authentication methods that are
        /// currently enabled in the specified namespace. The result includes configuration details for each method.</remarks>
        /// <param name="options">An optional <see cref="CommonOptions"/> object that specifies request parameters such as Fields and Expand</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
        /// wrapping an <see cref="AuthMethodsResponse"/> with details of the enabled authentication methods.</returns>
        Task<Result<AuthMethodsResponse>> ListAuthMethodsAsync(CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Authenticates an admin using email and password.
        /// </summary>
        /// <param name="email">Admin email or identity.</param>
        /// <param name="password">Admin password.</param>
        /// <param name="identityField">A specific identity field to use (by default fallbacks to the first matching one).</param>
        /// <param name="options">An optional <see cref="CommonOptions"/> object that specifies request parameters such as Fields and Expand</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The authentication response containing the session token.</returns>
        Task<Result<AuthResponse>> AuthWithPasswordAsync(string email, string password, string? identityField = null, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Authenticates a user with OAuth2 provider and returns the authentication record.
        /// </summary>
        /// <param name="request">The OAuth2 authentication request containing provider, code, and other OAuth2 parameters.</param>
        /// <param name="options">An optional <see cref="CommonOptions"/> object that specifies request parameters such as Fields and Expand</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a Result object wrapping:
        /// - AuthRecordResponse containing the JWT token, user record data, and OAuth2 metadata on success
        /// - Error details including validation errors, network issues, or authentication failures on failure
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the underlying HTTP request fails due to network issues.</exception>
        Task<Result<AuthRecordResponse>> AuthWithOAuth2CodeAsync(AuthWithOAuth2Request request, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a one-time password (OTP) to be sent to the user's email for authentication.
        /// </summary>
        /// <param name="email">E-mail for sending the OTP code.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The requested OTP id.</returns>
        Task<Result<string>> RequestOtpAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Authenticates a user using a one-time password (OTP) code.
        /// </summary>
        /// <param name="otpId">The unique identifier associated with the OTP challenge. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="otpCode">The OTP code provided by the user. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="options">An optional <see cref="CommonOptions"/> object that specifies request parameters such as Fields and Expand</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> with
        /// the outcome of the authentication attempt, including the authentication response if successful.</returns>
        Task<Result<UserResponse>> AuthWithOtpAsync(string otpId, string otpCode, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the currently authenticated admin session.
        /// </summary>
        /// <param name="options">An optional <see cref="CommonOptions"/> object that specifies request parameters such as Fields and Expand</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated authentication response.</returns>
        Task<Result<AuthResponse>> AuthRefreshAsync(CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a verification email to be sent to the specified email address.
        /// </summary>
        /// <param name="email">The auth record email address to send the verification request (if exists).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<Result> RequestVerificationAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirms an email address using a verification token.
        /// </summary>
        /// <param name="token">The token from the verification request email.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<Result> ConfirmVerificationAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a password reset email to be sent to the specified email address.
        /// </summary>
        /// <param name="email">The auth record email address to send the password reset request (if exists).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirms a password reset using the provided token and sets a new password.
        /// </summary>
        /// <param name="token">The token from the password reset request email.</param>
        /// <param name="password">The new password to set.</param>
        /// <param name="passwordConfirm">The new password confirmation.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<Result> ConfirmPasswordResetAsync(string token, string password, string passwordConfirm, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests an email change confirmation to be sent to the specified new email address.
        /// </summary>
        /// <param name="newEmail">The new email address to send the change email request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<Result> RequestEmailChangeAsync(string newEmail, CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirms an email change using the provided token and password.
        /// </summary>
        /// <param name="token">The token from the change email request email.</param>
        /// <param name="password">	The account password to confirm the email change.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<Result> ConfirmEmailChangeAsync(string token, string password, CancellationToken cancellationToken = default);

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
