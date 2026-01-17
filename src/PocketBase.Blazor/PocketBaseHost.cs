using System;
using System.Diagnostics;
using System.Net.Http;
using PocketBase.Blazor;

namespace PocketBase.Blazor
{
    public sealed class PocketBaseHost : IDisposable
    {
        private readonly Process _process;

        public PocketBaseHost(string executablePath, int port = 8090)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = $"serve --http=127.0.0.1:{port}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            _process.Start();
        }

        public void Dispose()
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit();
            }
        }
    }
}

// Usage example
//using var pbHost = new PocketBaseHost(
//    executablePath: PocketBaseBinaryResolver.Resolve(),
//    port: 8090
//);

//var http = new HttpClient
//{
//    BaseAddress = new Uri("http://127.0.0.1:8090")
//};

//var cronClient = new CronClient(http);

//await cronClient.ReloadAsync();

//// PB runs while your app runs
