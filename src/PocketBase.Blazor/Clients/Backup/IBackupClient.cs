using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Backup
{
    public interface IBackupClient
    {
        Task<Result<IEnumerable<BackupModel>>> GetFullListAsync(CancellationToken cancellationToken = default);
    }
}
