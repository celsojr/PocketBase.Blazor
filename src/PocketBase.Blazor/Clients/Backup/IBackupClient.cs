using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Backup;

namespace PocketBase.Blazor.Clients.Backup
{
    /// <summary>
    /// Provides access to PocketBase backup management APIs.
    /// Mirrors the behavior of the JS SDK BackupService.
    /// </summary>
    public interface IBackupClient
    {
        /// <summary>
        /// Returns the full list of available backups.
        /// </summary>
        /// <param name="option">Common options for the request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of backup metadata.</returns>
        Task<Result<List<BackupInfoResponse>>> GetFullListAsync(CommonOptions? option = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new backup.
        /// </summary>
        /// <param name="basename">
        /// Optional base name for the generated backup file.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>true</c> if the backup was successfully created.
        /// </returns>
        Task<Result<bool>> CreateAsync(string? basename = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads an existing backup archive.
        /// </summary>
        /// <param name="file">
        /// Backup archive file (ZIP).
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>true</c> if the upload completed successfully.
        /// </returns>
        Task<Result<bool>> UploadAsync(MultipartFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a backup by its key.
        /// </summary>
        /// <param name="key">Backup key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>true</c> if the backup was deleted successfully.
        /// </returns>
        Task<Result<bool>> DeleteAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores the application state from a backup.
        /// </summary>
        /// <param name="key">Backup key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>true</c> if the restore operation started successfully.
        /// </returns>
        Task<Result<bool>> RestoreAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Builds a download URL for a backup file.
        /// </summary>
        /// <param name="key">Backup key.</param>
        /// <param name="token">Superuser file token.</param>
        /// <returns>A relative download URL.</returns>
        string GetDownloadUrl(string key, string token);
    }
}

