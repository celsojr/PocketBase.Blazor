using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Options;
using PbOptions = PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Files
{
    /// <summary>
    /// Client for managing PocketBase file operations.
    /// </summary>
    public interface IFilesClient
    {
        /// <summary>
        /// Builds and returns an absolute record file url for the provided filename.
        /// </summary>
        /// <param name="collectionId">The collection ID.</param>
        /// <param name="recordId">The record ID.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="options">Optional file options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<Result<string>> GetUrl(string collectionId, string recordId, string fileName, PbOptions.FileOptions options = null!, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a new private file access token for the current auth model.
        /// </summary>
        /// <param name="options">The common options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<Result<string>> GetTokenAsync(CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads the specified file as a stream.
        /// </summary>
        /// <param name="collectionId">The collection ID.</param>
        /// <param name="recordId">The record ID.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="options">Optional file options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<Result<Stream>> GetStreamAsync(string collectionId, string recordId, string fileName, PbOptions.FileOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads the specified file as a byte array.
        /// </summary>
        /// <param name="collectionId">The collection ID.</param>
        /// <param name="recordId">The record ID.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="options">Optional file options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<Result<byte[]>> GetBytesAsync(string collectionId, string recordId, string fileName, PbOptions.FileOptions? options = null, CancellationToken cancellationToken = default);
    }
}
