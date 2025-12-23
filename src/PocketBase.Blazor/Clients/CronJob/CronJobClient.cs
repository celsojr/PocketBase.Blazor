using System.Threading.Tasks;
using PocketBase.Blazor.Options;
using System.Collections.Generic;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.CronJob
{
    /// <inheritdoc />
    public class CronJobClient : ICronJobClient
    {
        /// <inheritdoc />
        public Task<IEnumerable<CronJobResponse>> GetFullList(CommonOptions? options = null)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task Run(string id, CommonOptions? options = null)
        {
            throw new System.NotImplementedException();
        }
    }
}

