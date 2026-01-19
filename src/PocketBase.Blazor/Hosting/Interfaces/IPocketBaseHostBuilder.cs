using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Hosting.Interfaces
{
    public interface IPocketBaseHostBuilder
    {
        IPocketBaseHostBuilder UseExecutable(string executablePath);
        IPocketBaseHostBuilder UseOptions(Action<PocketBaseHostOptions> configure);
        IPocketBaseHostBuilder UseLogger(ILogger<PocketBaseHost> logger);
        IPocketBaseHostBuilder UseEnvironmentVariables(string prefix = "POCKETBASE_");
        IPocketBaseHostBuilder UseConfigurationFile(string file);
        Task<IPocketBaseHost> BuildAsync();
    }
}
