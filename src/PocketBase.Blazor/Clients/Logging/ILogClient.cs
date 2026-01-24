using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Logging;

namespace PocketBase.Blazor.Clients.Logging
{
    /// <summary>
    /// Client for managing application logs.
    /// </summary>
    public interface ILogClient
    {
        /// <summary>
        /// Returns paginated logs list.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="perPage">The number of items per page.</param>
        /// <param name="options">Additional options for the request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<ListResult<LogResponse>>> GetListAsync(int page = 1, int perPage = 30, ListOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a single log by its id.
        /// If `id` is empty it will throw a 404 error.
        /// </summary>
        /// <param name="id">The ID of the log entry to retrieve.</param>
        /// <param name="options">Additional options for the request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<LogResponse>> GetOneAsync(string id, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns hourly statistics for logs.
        /// </summary>
        /// <param name="options">Additional options for the request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<List<HourlyStatsResponse>>> GetStatsAsync(LogStatsOptions? options = null, CancellationToken cancellationToken = default);
    }
}

