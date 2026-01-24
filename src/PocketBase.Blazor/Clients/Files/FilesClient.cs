using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Auth;

namespace PocketBase.Blazor.Clients.Files
{
    /// <inheritdoc />
    public class FilesClient : IFilesClient
    {
        readonly IHttpTransport _http;

        /// <inheritdoc />
        public FilesClient(IHttpTransport http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public async Task<Result<string>> GetUrl(IDictionary<string, object?> record, string fileName, IDictionary<string, object?>? query, CancellationToken cancellationToken = default)
        {
            object? collectionName = null;

            if (string.IsNullOrWhiteSpace(fileName) ||
                !record.TryGetValue("id", out var id) ||
                !(record.TryGetValue("collectionId", out var collectionId) ||
                record.TryGetValue("collectionName", out collectionName)))
            {
                return string.Empty;
            }

            var path = string.Join('/', [
                "api",
                "files",
                Uri.EscapeDataString((collectionId ?? collectionName)!.ToString()!),
                Uri.EscapeDataString(id!.ToString()!),
                Uri.EscapeDataString(fileName)
            ]);

            // normalize the download query param (same as TS)
            if (query != null &&
                query.TryGetValue("download", out var download) &&
                download?.Equals("false") == true)
            {
                query.Remove("download");
            }

            var request = await _http.SendAsync<string>(
                HttpMethod.Get,
                path,
                query: query,
                cancellationToken: cancellationToken
            );

            return request ?? string.Empty;
        }

        /// <inheritdoc />
        public async Task<Result<string>> GetTokenAsync(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new CommonOptions();

            var data = await _http.SendAsync<TokenResponse>(
                HttpMethod.Post,
                "api/files/token",
                body: options.Body,
                query: options.Query,
                cancellationToken: cancellationToken
            );

            return data?.Value.Token ?? string.Empty;
        }
    }
}
