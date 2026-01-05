using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

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

        /// <summary>
        /// Gets a paginated list of items of type T.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="perPage">The number of items per page.</param>
        /// <param name="options">Additional options for the request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the paginated list of items.</returns>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<ListResult<T>>> GetListAsync<T>(int page = 1, int perPage = 30, ListOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single item of type T by its ID.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="options">Additional options for the request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the requested item.</returns>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<T>> GetOneAsync<T>(string? id, CommonOptions? options = null, CancellationToken cancellationToken = default);
    }
}

