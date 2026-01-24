using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Requests;
using PocketBase.Blazor.Responses.Cron;

namespace PocketBase.Blazor.Clients.Crons
{
    /// <inheritdoc />
    public class CronsClient : ICronsClient
    {
        private readonly IHttpTransport _http;

        /// <inheritdoc />
        public CronsClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <inheritdoc />
        public Task<Result<IEnumerable<CronsResponse>>> GetFullListAsync(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            var query = options?.ToDictionary();
            return _http.SendAsync<IEnumerable<CronsResponse>>(HttpMethod.Get, "api/crons", query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Result> RunAsync(string id, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Cron job id is required.", nameof(id));

            var query = options?.ToDictionary();
            return _http.SendAsync(HttpMethod.Post, $"api/crons/{id}", body: null, query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Result<CronRegisterResponse>> RegisterAsync(CronRegisterRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentException("Cron id is required.", nameof(request.Id));

            if (string.IsNullOrWhiteSpace(request.Expression))
                throw new ArgumentException("Cron expression is required.", nameof(request.Expression));

            return _http.SendAsync<CronRegisterResponse>(HttpMethod.Post, "internal/cron", body: request, cancellationToken: cancellationToken);
        }
    }
}

