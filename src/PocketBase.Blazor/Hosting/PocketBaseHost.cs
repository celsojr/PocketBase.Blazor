using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PocketBase.Blazor.Hosting.Interfaces;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Hosting
{
    public sealed class PocketBaseHost : IPocketBaseHost
    {
        private Process _process = null!;
        private readonly ILogger<PocketBaseHost> _logger;
        private readonly PocketBaseHostOptions _options;
        private readonly CancellationTokenSource _cts = new();
        private readonly string _executablePath;
        private bool _isRunning = false;
        private readonly SemaphoreSlim _startLock = new(1, 1);

        //public string BaseUrl => $"http://{_options.Host}:{_options.Port}";
        public string BaseUrl => "http://127.0.0.1:8090";

        public PocketBaseHostOptions? Options => _options;

        public Process? Process => _process;

        public string? ExecutablePath => _executablePath;

        public PocketBaseHost(string executablePath, PocketBaseHostOptions? options = null, ILogger<PocketBaseHost>? logger = null)
        {
            _options = options ?? new PocketBaseHostOptions();
            _logger = logger ?? NullLogger<PocketBaseHost>.Instance;

            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                throw new FileNotFoundException("PocketBase executable not found", executablePath);

            _executablePath = executablePath;

            var args = BuildArguments();
            _process = CreateNewProcess(executablePath, args);
        }

        private Process CreateNewProcess(string executablePath, string args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += OnOutputDataReceived;
            process.ErrorDataReceived += OnErrorDataReceived;
    
            return process;
        }

        private string BuildArguments()
        {
            // Host and Port are still paused under investigation
            //var args = $"serve --http={_options.Host}:{_options.Port}";

            var args = "serve";

            if (_options.Dev)
                args += " --dev";

            if (!string.IsNullOrWhiteSpace(_options.Dir))
                args += $" --dir=\"{_options.Dir}\"";

            if (!string.IsNullOrWhiteSpace(_options.MigrationsDir))
                args += $" --migrationsDir=\"{_options.MigrationsDir}\"";

            return args;
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                _logger.LogInformation("[PocketBase] {Message}", e.Data);
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                _logger.LogError("[PocketBase] {Message}", e.Data);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _startLock.WaitAsync(cancellationToken);
            try
            {
                if (_isRunning)
                {
                    throw new InvalidOperationException(
                        "PocketBase host is already running. Call StopAsync() first.");
                }

                _logger.LogInformation("Starting PocketBase on {BaseUrl}", BaseUrl);

                var args = BuildArguments();
                ArgumentException.ThrowIfNullOrWhiteSpace(ExecutablePath);
                _process = CreateNewProcess(ExecutablePath, args);

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                // Wait for PocketBase to start up
                await Task.Delay(1000, cancellationToken);

                _isRunning = true;
                _logger.LogInformation("PocketBase started successfully!");
            }
            finally
            {
                _startLock.Release();
            }
        }

        public async Task RestartAsync(CancellationToken cancellationToken = default)
        {
            await DisposeAsync();
            await Task.Delay(500, cancellationToken); // Brief cooldown
            await StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _startLock.WaitAsync(cancellationToken);
            try
            {
                if (!_isRunning || _process == null)
                    return;

                _logger.LogInformation("Stopping PocketBase...");

                if (!_process.HasExited)
                {
                    try
                    {
                        _process.Kill(entireProcessTree: true);
                    }
                    catch (InvalidOperationException)
                    {
                        // Already exited
                    }

                    await _process.WaitForExitAsync(cancellationToken);
                }

                _isRunning = false;
                _logger.LogInformation("PocketBase stopped");
            }
            finally
            {
                _startLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();

            _process.OutputDataReceived -= OnOutputDataReceived;
            _process.ErrorDataReceived -= OnErrorDataReceived;
            _process.Dispose();
            _process = null!;
            _cts.Dispose();
        }
    }
}
