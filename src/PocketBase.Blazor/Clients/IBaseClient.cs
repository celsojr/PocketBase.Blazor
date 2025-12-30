using PocketBase.Blazor.Http;

namespace PocketBase.Blazor.Clients
{
    /// <summary>
    /// Represents the base client interface for PocketBase API interactions.
    /// </summary>
    public interface IBaseClient
    {
        /// <summary>
        /// Gets the HTTP transport used for making API requests.
        /// </summary>
        IHttpTransport Http { get; }

        /// <summary>
        /// Encodes a URL parameter.
        /// </summary>
        /// <param name="param">The parameter to encode.</param>
        /// <returns>The encoded parameter.</returns>
        string UrlEncode(string? param);
    }
}

