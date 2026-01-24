using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Auth;
using PocketBase.Blazor.Responses.Settings;

namespace PocketBase.Blazor.Clients.Settings
{
    /// <summary>
    /// Client for managing application settings.
    /// </summary>
    public interface ISettingsClient
    {
        /// <summary>
        /// Fetch all available app settings.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<SettingsResponse>> GetListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk updates app settings.
        /// </summary>
        /// <param name="settings">The settings to update.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result> UpdateAsync(object settings, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs an S3 filesystem connection test.
        /// 
        /// The currently supported filesystems are "storage" and "backups".
        /// </summary>
        /// <param name="fileSystem">
        /// The filesystem to test. Defaults to "storage".
        /// </param>
        /// <param name="options">
        /// Optional parameters for the connection test.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional cancellation token.
        /// </param>
        /// <returns>
        /// True if the connection test succeeds; otherwise false.
        /// </returns>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<bool>> TestS3Async(string fileSystem = "storage", CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a test email.
        /// 
        /// The possible `emailTemplate` values are:
        /// - verification
        /// - password-reset
        /// - email-change
        /// </summary>
        /// <param name="collectionIdOrName">
        /// The ID or name of the collection to test.
        /// </param>
        /// <param name="toEmail">
        /// The recipient email address.
        /// </param>
        /// <param name="emailTemplate">
        /// The email template to use.
        /// </param>
        /// <param name="options">
        /// Optional parameters for the email test.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional cancellation token.
        /// </param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<bool>> TestEmailAsync(string collectionIdOrName, string toEmail, string emailTemplate, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a new Apple OAuth2 client secret.
        /// </summary>
        /// <param name="clientId">
        /// The ID of the Apple OAuth2 client.
        /// </param>
        /// <param name="teamId">
        /// The ID of the Apple Developer Team.
        /// </param>
        /// <param name="keyId">
        /// The ID of the Apple Key.
        /// </param>
        /// <param name="privateKey">
        /// The private key for the Apple Key.
        /// </param>
        /// <param name="duration">
        /// The duration for the client secret.
        /// </param>
        /// <param name="options">
        /// Optional parameters for the client secret generation.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional cancellation token.
        /// </param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<AppleClientSecretResponse>> GenerateAppleClientSecretAsync(string clientId, string teamId, string keyId, string privateKey, int duration, CommonOptions? options, CancellationToken cancellationToken = default);
    }
}

