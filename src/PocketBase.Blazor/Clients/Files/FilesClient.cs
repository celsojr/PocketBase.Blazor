using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PbOptions = PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Auth;
using System.Linq;
using System.Collections.Generic;

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
        public async Task<Result<string>> GetUrl(string collectionId, string recordId, string fileName, PbOptions.FileOptions options = null!, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName) ||
                string.IsNullOrWhiteSpace(recordId))
            {
                return string.Empty;
            }

            string path = string.Join('/', [
                "api",
                "files",
                Uri.EscapeDataString(collectionId),
                Uri.EscapeDataString(recordId),
                Uri.EscapeDataString(fileName)
            ]);

            options ??= new PbOptions.FileOptions();
            Dictionary<string, object?>? query = options.BuildQuery();

            if (query != null)
            {
                // normalize the download query param (same as TS)
                if (query.TryGetValue("download", out object? download)
                    && download?.Equals("false") == true)
                {
                    query.Remove("download");
                }

                query.Remove("page");
                query.Remove("perPage");
            }

            if (query != null && query.Count > 0)
                path += "?" + string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"));

            return path;
        }

        /// <inheritdoc />
        public async Task<Result<string>> GetTokenAsync(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new CommonOptions();

            Result<TokenResponse> data = await _http.SendAsync<TokenResponse>(
                HttpMethod.Post,
                "api/files/token",
                body: options.Body,
                query: options.Query,
                cancellationToken: cancellationToken
            );

            return data?.Value.Token ?? string.Empty;
        }

        /// <inheritdoc />
        public async Task<Result<Stream>> GetStreamAsync(string collectionId, string recordId, string fileName, PbOptions.FileOptions? options = null, CancellationToken cancellationToken = default)
        {
           if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(recordId))
           {
               return Result.Fail<Stream>("fileName and recordId are required");
           }

            string path = string.Join('/', [
               "api",
               "files",
               Uri.EscapeDataString(collectionId),
               Uri.EscapeDataString(recordId),
               Uri.EscapeDataString(fileName)
           ]);

           options ??= new PbOptions.FileOptions();
           Dictionary<string, object?> query = options.BuildQuery();

           return await _http.SendForStreamAsync(HttpMethod.Get, path, query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result<byte[]>> GetBytesAsync(string collectionId, string recordId, string fileName, PbOptions.FileOptions? options = null!, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(recordId))
           {
               return Result.Fail<byte[]>("fileName and recordId are required");
           }

            string path = string.Join('/', [
               "api",
               "files",
               Uri.EscapeDataString(collectionId),
               Uri.EscapeDataString(recordId),
               Uri.EscapeDataString(fileName)
           ]);

           options ??= new PbOptions.FileOptions();
            Dictionary<string, object?> query = options.BuildQuery();

           return await _http.SendForBytesAsync(HttpMethod.Get, path, query: query, cancellationToken: cancellationToken);
        }
    }
}
