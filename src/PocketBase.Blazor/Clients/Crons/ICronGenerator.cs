using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Crons
{
    public interface ICronGenerator
    {
        Task GenerateServerProjectFilesAsync(CronGenerationOptions options, CancellationToken cancellationToken = default);
        Task GenerateHandlersAsync(CronManifest manifest, string outputDir, CronGenerationOptions options, CancellationToken cancellationToken = default);
        Task GenerateAsync(CronManifest cronManifest, CronGenerationOptions options, CancellationToken cancellationToken = default);
        Task BuildGoBinaryAsync(CronGenerationOptions options, CancellationToken cancellationToken = default);
    }
}

