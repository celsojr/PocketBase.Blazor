using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PocketBase.Blazor.Hosting.Interfaces;
using PocketBase.Blazor.Hosting.Services;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Hosting
{
    public class PocketBaseHostBuilder : IPocketBaseHostBuilder
    {
        private string? _executablePath;
        private PocketBaseHostOptions _options = new();
        private ILogger<PocketBaseHost>? _logger;

        public static IPocketBaseHostBuilder CreateDefault(string[]? args = null)
        {
            return new PocketBaseHostBuilder();
        }

        public IPocketBaseHostBuilder UseExecutable(string executablePath)
        {
            _executablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
            return this;
        }

        public IPocketBaseHostBuilder UseOptions(Action<PocketBaseHostOptions> configure)
        {
            configure?.Invoke(_options);
            return this;
        }

        public IPocketBaseHostBuilder UseLogger(ILogger<PocketBaseHost> logger)
        {
            _logger = logger;
            return this;
        }

        public async Task<IPocketBaseHost> BuildAsync()
        {
            // If no executable specified, try to resolve it
            _executablePath ??= await PocketBaseBinaryResolver.ResolveAsync();

            if (string.IsNullOrWhiteSpace(_executablePath))
                throw new InvalidOperationException("No PocketBase executable found");

            return new PocketBaseHost(_executablePath, _options, _logger);
        }
    }
}


/*

// Startup
var builder = PocketBaseHostBuilder.CreateDefault();

// Auto-resolve and download executable if needed
await builder
    .UseOptions(options =>
    {
        options.Host = "localhost";
        options.Port = 8090;
        options.Dir = "./pb_data";
        options.Dev = true;
    })
    .BuildAsync();

// Or specify executable directly
await builder
    .UseExecutable(@"C:\tools\pocketbase.exe")
    .UseOptions(options => options.Port = 8090)
    .BuildAsync();

// Create client
var host = await builder.BuildAsync();
await host.StartAsync();

var pb = PocketBaseClientFactory.CreateClient(host);

// Use the client
var auth = await pb.Admins
    .AuthWithPasswordAsync(
        "admin@example.com",
        "admin123"
    );

// Dispose when done
await host.DisposeAsync();
 
*/

