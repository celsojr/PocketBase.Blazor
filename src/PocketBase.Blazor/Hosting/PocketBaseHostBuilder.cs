using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
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
            if (_logger != null)
                PocketBaseBinaryResolver.SetLogger(_logger);

            // If no executable specified, try to resolve it
            _executablePath ??= await PocketBaseBinaryResolver.ResolveAsync();

            if (string.IsNullOrWhiteSpace(_executablePath))
                throw new InvalidOperationException("No PocketBase executable found");

            return new PocketBaseHost(_executablePath, _options, _logger);
        }

        public IPocketBaseHostBuilder UseEnvironmentVariables(string prefix = "POCKETBASE_")
        {
            var envVars = Environment.GetEnvironmentVariables();

            foreach (DictionaryEntry envVar in envVars)
            {
                var key = envVar.Key.ToString();

                if (key?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true)
                {
                    var optionName = key.Substring(prefix.Length);
                    var value = envVar.Value?.ToString();

                    if (value != null)
                        ApplyEnvironmentVariable(optionName, value);
                }
            }

            return this;
        }

        private void ApplyEnvironmentVariable(string optionName, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            switch (optionName.ToUpperInvariant())
            {
                case "HOST":
                    _options.Host = value;
                    break;

                case "PORT":
                    if (int.TryParse(value, out int port))
                        _options.Port = port;
                    break;

                case "DIR":
                    _options.Dir = value;
                    break;

                case "DEV":
                    if (bool.TryParse(value, out bool dev))
                        _options.Dev = dev;
                    break;
            }
        }

        public IPocketBaseHostBuilder UseConfigurationFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".json" => UseJsonConfiguration(filePath),
                //".yaml" or ".yml" => UseYamlConfiguration(filePath),
                //".toml" => UseTomlConfiguration(filePath),
                ".xml" => UseXmlConfiguration(filePath),
                //".ini" => UseIniConfiguration(filePath),
                _ => throw new NotSupportedException($"Unsupported configuration file format: {extension}")
            };
        }

        public PocketBaseHostBuilder UseJsonConfiguration(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<PocketBaseHostOptions>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            ApplyConfiguration(config);
            return this;
        }

        public IPocketBaseHostBuilder UseYamlConfiguration(string filePath)
        {
            throw new NotImplementedException();
        }

        //public PocketBaseHostBuilder UseYamlConfiguration(string filePath)
        //{
        //    // Requires YamlDotNet package
        //    // Install-Package YamlDotNet
        //    var yaml = File.ReadAllText(filePath);
        //    var deserializer = new DeserializerBuilder().Build();
        //    var config = deserializer.Deserialize<Dictionary<string, object>>(yaml);

        //    ApplyDictionaryConfiguration(config);
        //    return this;
        //}

        public IPocketBaseHostBuilder UseXmlConfiguration(string filePath)
        {
            var xml = File.ReadAllText(filePath);
            var doc = XDocument.Parse(xml);

            var config = new PocketBaseHostOptions();

            var root = doc.Root;
            if (root != null)
            {
                config.Host = root.Element("Host")?.Value ?? config.Host;

                if (int.TryParse(root.Element("Port")?.Value, out int port))
                    config.Port = port;

                config.Dir = root.Element("Dir")?.Value ?? config.Dir;

                if (bool.TryParse(root.Element("Dev")?.Value, out bool dev))
                    config.Dev = dev;

                _executablePath = root.Element("Executable")?.Value ?? _executablePath;
            }

            ApplyConfiguration(config);
            return this;
        }

        private void ApplyConfiguration(PocketBaseHostOptions config)
        {
            if (config == null)
                return;

            _options.Host = config.Host ?? _options.Host;
            _options.Port = config.Port;
            _options.Dir = config.Dir ?? _options.Dir;
            _options.Dev = config.Dev;
            _executablePath = config.Executable ?? _executablePath;
        }

        private void ApplyDictionaryConfiguration(Dictionary<string, object> config)
        {
            if (config == null)
                return;

            if (config.TryGetValue("Host", out var host) && host is string hostStr)
                _options.Host = hostStr;

            if (config.TryGetValue("Port", out var port))
            {
                if (port is int portInt)
                    _options.Port = portInt;
                else if (port is string portStr && int.TryParse(portStr, out portInt))
                    _options.Port = portInt;
            }
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

