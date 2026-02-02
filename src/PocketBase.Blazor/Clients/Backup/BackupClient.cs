using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Backup;

namespace PocketBase.Blazor.Clients.Backup
{
    /// <inheritdoc />
    public sealed class BackupClient : IBackupClient
    {
        private readonly IHttpTransport _transport;

        /// <inheritdoc />
        public BackupClient(IHttpTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        /// <inheritdoc />
        public Task<Result<List<BackupInfoResponse>>> GetFullListAsync(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            var query = default(Dictionary<string, object?>);
            if (options != null)
            {
                query = options.BuildQuery();
                query.Remove("page");
                query.Remove("perPage");
            }
            return _transport.SendAsync<List<BackupInfoResponse>>(HttpMethod.Get, "api/backups", query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result<bool>> CreateAsync(string? basename = null, CancellationToken cancellationToken = default)
        {
            object? body = basename != null
                ? new { name = basename }
                : null;

            await _transport.SendAsync(HttpMethod.Post, "api/backups", body, cancellationToken: cancellationToken);

            return Result.Ok(true);
        }

        /// <inheritdoc />
        public async Task<Result<bool>> UploadAsync(MultipartFile file, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(file);

            await _transport.SendAsync(HttpMethod.Post, "api/backups/upload", file, cancellationToken: cancellationToken);

            return Result.Ok(true);
        }

        /// <inheritdoc />
        public async Task<Result<bool>> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Backup key is required.", nameof(key));
            }

            await _transport.SendAsync(HttpMethod.Delete, $"api/backups/{Uri.EscapeDataString(key)}", cancellationToken: cancellationToken);

            return Result.Ok(true);
        }

        /// <inheritdoc />
        public async Task<Result<bool>> RestoreAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Backup key is required.", nameof(key));
            }

            await _transport.SendAsync(HttpMethod.Post, $"api/backups/{Uri.EscapeDataString(key)}/restore", cancellationToken: cancellationToken);

            return Result.Ok(true);
        }

        /// <inheritdoc />
        public string GetDownloadUrl(string key, string token)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Backup key is required.", nameof(key));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token is required.", nameof(token));
            }

            return $"api/backups/{Uri.EscapeDataString(key)}?token={Uri.EscapeDataString(token)}";
        }
    }
}
