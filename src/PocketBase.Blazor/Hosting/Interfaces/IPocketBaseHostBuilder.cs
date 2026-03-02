using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PocketBase.Blazor.Clients.Crons;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Scaffolding;

namespace PocketBase.Blazor.Hosting.Interfaces
{
    /// <summary>
    /// Builder used to configure and create a <see cref="IPocketBaseHost"/> instance.
    /// </summary>
    public interface IPocketBaseHostBuilder
    {
        /// <summary>
        /// Sets the path to the PocketBase executable.
        /// </summary>
        IPocketBaseHostBuilder UseExecutable(string executablePath);

        /// <summary>
        /// Adds cron generation support.
        /// </summary>
        IPocketBaseHostBuilder UseCrons(ICronGenerator cronGenerator, CronManifest manifest, CronGenerationOptions options);

        /// <summary>
        /// Adds a single common schema template.
        /// </summary>
        IPocketBaseHostBuilder UseSchemaTemplate(CommonSchema schema);

        /// <summary>
        /// Adds multiple common schema templates.
        /// </summary>
        IPocketBaseHostBuilder UseSchemaTemplates(IEnumerable<CommonSchema> schemas);

        /// <summary>
        /// Configures host options.
        /// </summary>
        IPocketBaseHostBuilder UseOptions(Action<PocketBaseHostOptions> configure);

        /// <summary>
        /// Sets a logger instance for the host.
        /// </summary>
        IPocketBaseHostBuilder UseLogger(ILogger<PocketBaseHost> logger);

        /// <summary>
        /// Enables reading configuration from environment variables with an optional prefix.
        /// </summary>
        IPocketBaseHostBuilder UseEnvironmentVariables(string prefix = "POCKETBASE_");

        /// <summary>
        /// Loads configuration from a YAML file.
        /// </summary>
        IPocketBaseHostBuilder UseYamlConfiguration(string filePath);

        /// <summary>
        /// Loads configuration from a configuration file.
        /// </summary>
        IPocketBaseHostBuilder UseConfigurationFile(string filePath);

        /// <summary>
        /// Configures the application to use settings from a JSON configuration file at the specified path.
        /// </summary>
        /// <param name="filePath">The path to the JSON configuration file.</param>
        IPocketBaseHostBuilder UseJsonConfiguration(string filePath);

        /// <summary>
        /// Builds the host asynchronously.
        /// </summary>
        /// <returns>A task that resolves to the configured <see cref="IPocketBaseHost"/>.</returns>
        Task<IPocketBaseHost> BuildAsync();
    }
}
