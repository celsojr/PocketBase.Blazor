using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Backup
{
    public class BackupClient : IBackupClient
    {
        private IHttpTransport _http;

        public BackupClient(IHttpTransport http)
        {
            _http = http;
        }

        public Task<Result<IEnumerable<BackupModel>>> GetFullListAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

    }
}
