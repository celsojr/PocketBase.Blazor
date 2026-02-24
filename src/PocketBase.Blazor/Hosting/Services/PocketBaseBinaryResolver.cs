using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PocketBase.Blazor.Hosting.Services
{
    public static class PocketBaseBinaryResolver
    {
        private static readonly HttpClient _httpClient = new();
        private static ILogger? _logger = NullLogger.Instance;
        private const string Version = "0.34.0";
        private const string BaseDownloadUrl = "https://github.com/pocketbase/pocketbase/releases/download";

        public static ILogger Logger
        {
            get => _logger ?? NullLogger.Instance;
            set => _logger = value;
        }

        public static void SetLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static async Task<string?> ResolveAsync()
        {
            // Check if already downloaded
            string localPath = GetLocalExecutablePath();
            if (File.Exists(localPath))
                return localPath;

            // Download if not exists
            return await DownloadAsync();
        }

        private static string GetLocalExecutablePath()
        {
            OSPlatform os = GetOSPlatform();
            string extension = os == OSPlatform.Windows ? ".exe" : "";

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "pocketbase",
                $"pocketbase{extension}"
            );
        }

        private static async Task<string?> DownloadAsync()
        {
            try
            {
                OSPlatform os = GetOSPlatform();
                string arch = GetArchitectureString();
                string platformName = GetOSPlatformName();

                // In future we're gonna provide our own custom GO build to be able to create cron jobs also from this library
                // https://github.com/pocketbase/pocketbase/releases/download/v0.34.0/pocketbase_0.34.0_windows_amd64.zip

                string downloadUrl = $"{BaseDownloadUrl}/v{Version}/pocketbase_{Version}_{platformName}_{arch}.zip";

                string tempFile = Path.GetTempFileName();
                string localPath = GetLocalExecutablePath();

                Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

                _logger.LogInformation("Downloading PocketBase from {Url}", downloadUrl);

                using HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = File.Create(tempFile))
                {
                    await stream.CopyToAsync(fileStream);
                }

                // Extract
                ZipFile.ExtractToDirectory(tempFile, Path.GetDirectoryName(localPath)!, true);

                // Manual clean up
                File.Delete(tempFile);

                return localPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download PocketBase");
                return null;
            }
        }

        private static OSPlatform GetOSPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OSPlatform.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OSPlatform.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OSPlatform.OSX;

            throw new PlatformNotSupportedException();
        }

        private static string GetOSPlatformName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "darwin";

            throw new PlatformNotSupportedException();
        }

        private static string GetArchitectureString()
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "amd64",
                Architecture.Arm64 => "arm64",
                //Architecture.X86 => "386",
                _ => throw new PlatformNotSupportedException()
            };
        }
    }
}

