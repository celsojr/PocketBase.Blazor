using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Logging
{
    /// <inheritdoc />
    public class LogClient : ILogClient
    {
        private readonly IHttpTransport _http;

        /// <inheritdoc />
        public LogClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <inheritdoc />
        public Task<Result<ListResult<LogResponse>>> GetListAsync(int page = 1, int perPage = 30, ListOptions? options = null, CancellationToken cancellationToken = default)
        {
            var query = options?.ToDictionary() ?? new Dictionary<string, object?>();
            query["page"] = page.ToString();
            query["perPage"] = perPage.ToString();

            return _http.SendAsync<ListResult<LogResponse>>(HttpMethod.Get, "api/logs", query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Result<LogResponse>> GetOneAsync(string id, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Log id is required.", nameof(id));

            var query = options?.ToDictionary();
            return _http.SendAsync<LogResponse>(HttpMethod.Get, $"api/logs/{id}", query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Result<HourlyStatsResponse>> GetStatsAsync(LogStatsOptions? options = null, CancellationToken cancellationToken = default)
        {
            var query = options?.ToDictionary();
            return _http.SendAsync<HourlyStatsResponse>(HttpMethod.Get, "api/logs/stats", query: query, cancellationToken: cancellationToken);
        }
    }
}

