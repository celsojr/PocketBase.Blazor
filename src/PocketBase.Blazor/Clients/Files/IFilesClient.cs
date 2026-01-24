using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Options;

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
        /// <param name="query">Optional query parameters.</param>
        /// <param name="cancellationToken"></param>
        Task<Result<byte[]>> GetUrl(string collectionId, string recordId, string fileName, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a new private file access token for the current auth model.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        Task<Result<string>> GetTokenAsync(CommonOptions? options = null, CancellationToken cancellationToken = default);
    }
}

