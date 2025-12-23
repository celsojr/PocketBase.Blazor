using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Logging
{
    /// <inheritdoc />
    public class LogClient : ILogClient
    {
        private IHttpTransport _http;

        /// <inheritdoc />
        public LogClient(IHttpTransport http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public Task<ListResult<LogResponse>> GetListAsync(int page = 1, int perPage = 30, ListOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<LogResponse> GetOneAsync(string id, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<HourlyStatsResponse> GetStatsAsync(LogStatsOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}

