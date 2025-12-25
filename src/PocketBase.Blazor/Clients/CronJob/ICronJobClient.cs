using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.CronJob
{
    /// <summary>
    /// Client interface for managing cron jobs in PocketBase.
    /// </summary>
    public interface ICronJobClient
    {
        /// <summary>
        /// Returns list with all registered cron jobs.
        /// </summary>
        /// <param name="options">Optional common options for the request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<IEnumerable<CronJobResponse>>> GetFullList(CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs a specific cron job.
        /// </summary>
        /// <param name="id">The ID of the cron job to run.</param>
        /// <param name="options">Optional common options for the request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<object>> Run(string id, CommonOptions? options = null, CancellationToken cancellationToken = default);
    }
}

