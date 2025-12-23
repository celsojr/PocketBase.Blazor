using System.Threading.Tasks;
using PocketBase.Blazor.Options;
using System.Collections.Generic;
using PocketBase.Blazor.Responses;
using PocketBase.Blazor.Http;
using System.Threading;

namespace PocketBase.Blazor.Clients.CronJob
{
    /// <inheritdoc />
    public class CronJobClient : ICronJobClient
    {
        private IHttpTransport _http;

        /// <inheritdoc />
        public CronJobClient(IHttpTransport http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public Task<IEnumerable<CronJobResponse>> GetFullList(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task Run(string id, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}

