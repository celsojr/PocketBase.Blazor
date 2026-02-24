using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Crons
{
    /// <summary>
    /// Defines operations to generate and optionally build a PocketBase cron server project.
    /// </summary>
    public interface ICronGenerator
    {
        /// <summary>
        /// Generates base server files required to host and register cron handlers.
        /// </summary>
        /// <param name="options">Generation and output settings for the cron server project.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A task that completes when server project files are generated.</returns>
        Task GenerateServerProjectFilesAsync(CronGenerationOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates cron handler source files from the provided manifest.
        /// </summary>
        /// <param name="manifest">Cron definitions used to generate handlers.</param>
        /// <param name="outputDir">Directory where generated handler files are written.</param>
        /// <param name="options">Generation settings used during handler generation.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A task that completes when handler files are generated.</returns>
        Task GenerateHandlersAsync(CronManifest manifest, string outputDir, CronGenerationOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs the full generation flow for the provided cron manifest.
        /// </summary>
        /// <param name="cronManifest">Cron manifest containing jobs to generate.</param>
        /// <param name="options">Generation and build settings.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A task that completes when generation finishes.</returns>
        Task GenerateAsync(CronManifest cronManifest, CronGenerationOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Builds the generated Go cron server binary.
        /// </summary>
        /// <param name="options">Build settings, including executable and output paths.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A task that completes when the Go build process finishes.</returns>
        Task BuildGoBinaryAsync(CronGenerationOptions options, CancellationToken cancellationToken = default);
    }
}
