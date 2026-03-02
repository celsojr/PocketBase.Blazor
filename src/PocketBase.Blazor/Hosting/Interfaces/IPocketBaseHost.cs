using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Hosting.Interfaces
{
    /// <summary>
    /// Represents a running PocketBase host process and control operations.
    /// </summary>
    public interface IPocketBaseHost : IAsyncDisposable
    {
        /// <summary>
        /// The base URL where the PocketBase instance is reachable.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// The options used to configure the host.
        /// </summary>
        PocketBaseHostOptions? Options { get; }

        /// <summary>
        /// The underlying <see cref="Process"/> running PocketBase, if available.
        /// </summary>
        Process? Process { get; }

        /// <summary>
        /// Path to the PocketBase executable being used, if specified.
        /// </summary>
        string? ExecutablePath { get; }

        /// <summary>
        /// Starts the host process.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Restarts the host process.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task RestartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the host process.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
