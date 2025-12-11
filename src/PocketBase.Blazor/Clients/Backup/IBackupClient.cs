using FluentResults;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;
using System.Collections.Generic;

namespace PocketBase.Blazor.Clients.Backup
{
    public interface IBackupClient
    {
        Task<Result<IEnumerable<BackupModel>>> GetFullListAsync();
    }
}
