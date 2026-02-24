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
    public interface IPocketBaseHostBuilder
    {
        IPocketBaseHostBuilder UseExecutable(string executablePath);
        IPocketBaseHostBuilder UseCrons(ICronGenerator cronGenerator, CronManifest manifest, CronGenerationOptions options);
        IPocketBaseHostBuilder UseSchemaTemplate(CommonSchema schema);
        IPocketBaseHostBuilder UseSchemaTemplates(IEnumerable<CommonSchema> schemas);
        IPocketBaseHostBuilder UseOptions(Action<PocketBaseHostOptions> configure);
        IPocketBaseHostBuilder UseLogger(ILogger<PocketBaseHost> logger);
        IPocketBaseHostBuilder UseEnvironmentVariables(string prefix = "POCKETBASE_");
        IPocketBaseHostBuilder UseYamlConfiguration(string filePath);
        IPocketBaseHostBuilder UseConfigurationFile(string filePath);
        Task<IPocketBaseHost> BuildAsync();
    }
}
