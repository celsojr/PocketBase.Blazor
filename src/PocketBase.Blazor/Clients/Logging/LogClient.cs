using System.Threading.Tasks;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Logging
{
    /// <inheritdoc />
    public class LogClient : ILogClient
    {
        /// <inheritdoc />
        public Task<ListResult<LogResponse>> GetListAsync(int page = 1, int perPage = 30, ListOptions? options = null)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<LogResponse> GetOneAsync(string id, CommonOptions? options = null)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<HourlyStatsResponse> GetStatsAsync(LogStatsOptions? options = null)
        {
            throw new System.NotImplementedException();
        }
    }
}

