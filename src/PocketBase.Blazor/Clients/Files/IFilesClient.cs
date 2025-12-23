using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// <param name="record">The record model.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="query">Optional query parameters.</param>
        Task<string> GetUrl(IDictionary<string, object?> record, string fileName, IDictionary<string, string>? query = null);

        /// <summary>
        /// Requests a new private file access token for the current auth model.
        /// </summary>
        /// <param name="options"></param>
        Task<string> GetTokenAsync(CommonOptions? options = null);
    }
}

