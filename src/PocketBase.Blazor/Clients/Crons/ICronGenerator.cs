using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Crons
{
    public interface ICronGenerator
    {
        Task GenerateAsync(CronManifest cronManifest,
            CronGenerationOptions options,
            CancellationToken cancellationToken = default);
    }
}

