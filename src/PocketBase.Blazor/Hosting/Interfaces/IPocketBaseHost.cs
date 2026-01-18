using System;
using System.Threading;
using System.Threading.Tasks;

namespace PocketBase.Blazor.Hosting.Interfaces
{
    public interface IPocketBaseHost : IAsyncDisposable
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
        string BaseUrl { get; }
    }
}
