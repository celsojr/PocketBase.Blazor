using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

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
        public async Task<string> GetUrl(IDictionary<string, object?> record, string fileName, IDictionary<string, string>? query)
        {
            object? collectionId = null, collectionName = null;

            if (string.IsNullOrWhiteSpace(fileName) ||
                !record.TryGetValue("id", out var id) ||
                !(record.TryGetValue("collectionId", out collectionId) ||
                record.TryGetValue("collectionName", out collectionName)))
            {
                return string.Empty;
            }

            var path = string.Join("/",
                "api",
                "files",
                Uri.EscapeDataString((collectionId ?? collectionName)!.ToString()!),
                Uri.EscapeDataString(id!.ToString()!),
                Uri.EscapeDataString(fileName)
            );

            // normalize the download query param (same as TS)
            if (query != null &&
                query.TryGetValue("download", out var download) &&
                download == "false")
            {
                query.Remove("download");
            }

            var request = await _http.SendAsync<string>(
                HttpMethod.Get,
                path,
                query: query
            );

            return request ?? string.Empty;
        }

        /// <inheritdoc />
        public async Task<string> GetTokenAsync(CommonOptions? options = null)
        {
            options ??= new CommonOptions();

            var method = options.Method ?? HttpMethod.Post;

            var data = await _http.SendAsync<TokenResponse>(
                method,
                "/api/files/token",
                body: options.Body,
                query: options.Query
            );

            return data?.Token ?? string.Empty;
        }
    }
}
