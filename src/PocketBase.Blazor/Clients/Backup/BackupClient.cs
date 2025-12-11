using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Backup
{
    public class BackupClient : IBackupClient
    {
        public Task<Result<IEnumerable<BackupModel>>> GetFullListAsync()
        {
            throw new System.NotImplementedException();
        }

    }
}
