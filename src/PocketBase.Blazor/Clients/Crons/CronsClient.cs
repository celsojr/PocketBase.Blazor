using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

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
            return _http.SendAsync(HttpMethod.Post, $"api/crons/{id}/run", body: null, query: query, cancellationToken: cancellationToken);
        }
    }
}

