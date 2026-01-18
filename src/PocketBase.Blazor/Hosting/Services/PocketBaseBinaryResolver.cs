using System;
using System.Diagnostics;
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

        public static async Task<string?> ResolveAsync()
        {
            // Check if already downloaded
            var localPath = GetLocalExecutablePath();
            if (File.Exists(localPath))
                return localPath;

            // Download if not exists
            return await DownloadAsync();
        }

        private static string GetLocalExecutablePath()
        {
            var os = GetOSPlatform();
            var arch = RuntimeInformation.ProcessArchitecture;
            var extension = os == OSPlatform.Windows ? ".exe" : "";

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "pocketbase",
                $"pocketbase_{os}_{arch}{extension}"
            );
        }

        private static async Task<string?> DownloadAsync()
        {
            try
            {
                var os = GetOSPlatform();
                var arch = GetArchitectureString();
                var extension = os == OSPlatform.Windows ? "zip" : "tar.gz";
                string platformName = GetOSPlatformName();

                // https://github.com/pocketbase/pocketbase/releases/download/v0.34.0/pocketbase_0.34.0_windows_amd64.zip

                var downloadUrl = $"{BaseDownloadUrl}/v{Version}/pocketbase_{Version}_{platformName}_{arch}.{extension}";

                var tempFile = Path.GetTempFileName();
                var localPath = GetLocalExecutablePath();

                Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

                _logger.LogInformation("Downloading PocketBase from {Url}", downloadUrl);

                using var response = await _httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(tempFile);
                await stream.CopyToAsync(fileStream);

                // Extract
                if (extension == "zip")
                {
                    ZipFile.ExtractToDirectory(tempFile, Path.GetDirectoryName(localPath)!, true);
                }
                else
                {
                    // For tar.gz on Linux/macOS (simplified - in reality use SharpCompress)
                    File.Move(tempFile, localPath, true);
                    if (os != OSPlatform.Windows)
                    {
                        Process.Start("chmod", $"+x {localPath}");
                    }
                }

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

