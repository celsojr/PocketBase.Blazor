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
        public async Task<Result> CreateAsync(string? basename = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(basename))
            {
                return await _transport.SendAsync(HttpMethod.Post, "api/backups", cancellationToken: cancellationToken);
            }

            var sanitizedName = SanitizeBackupName(basename);

            var body = new { name = sanitizedName };

            return await _transport.SendAsync(HttpMethod.Post, "api/backups", body, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result> UploadAsync(MultipartFile file, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(file);

            return await _transport.SendAsync(HttpMethod.Post, "api/backups/upload", file, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Backup key is required.", nameof(key));
            }

            var sanitizedKey = SanitizeBackupName(key);

            return await _transport.SendAsync(HttpMethod.Delete, $"api/backups/{Uri.EscapeDataString(sanitizedKey)}", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result> RestoreAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Backup key is required.", nameof(key));
            }

            var sanitizedKey = SanitizeBackupName(key);

            return await _transport.SendAsync(HttpMethod.Post, $"api/backups/{Uri.EscapeDataString(sanitizedKey)}/restore", cancellationToken: cancellationToken);
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

            var sanitizedKey = SanitizeBackupName(key);

            return _transport.BuildUrl($"/api/backups/{Uri.EscapeDataString(sanitizedKey)}?token={Uri.EscapeDataString(token)}");
        }

        private static string SanitizeBackupName(string filename)
        {
            // Trim whitespace
            filename = filename.Trim();

            // Remove .zip extension for sanitization
            if (filename.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                filename = filename[..^4];
            }
    
            // Convert to lowercase
            filename = filename.ToLowerInvariant();
    
            // Only keep valid characters
            var validChars = new List<char>();
            foreach (char c in filename)
            {
                if ((c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') ||
                    c == '_' ||
                    c == '-')
                {
                    validChars.Add(c);
                }
                else
                {
                    // Replace invalid characters with underscore
                    validChars.Add('_');
                }
            }
    
            var sanitized = new string([.. validChars]);
    
            // Ensure not empty after sanitization
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                // Generate a default name
                sanitized = $"backup_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}";
            }

            // Append .zip extension (required by Pocketbase)
            return sanitized += ".zip";
        }
    }
}
