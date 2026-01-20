using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Hosting.Interfaces
{
    public interface IPocketBaseHost : IAsyncDisposable
    {
        string BaseUrl { get; }
        PocketBaseHostOptions? Options { get; }
        Process? Process { get; }
        string? ExecutablePath { get; }

        Task StartAsync(CancellationToken cancellationToken = default);
        Task RestartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
