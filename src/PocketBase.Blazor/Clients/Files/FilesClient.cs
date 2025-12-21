using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using System.Collections.Generic;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Files
{
    public class FilesClient : IFilesClient
    {
        readonly IHttpTransport _http;

        public FilesClient(IHttpTransport http)
        {
            _http = http;
        }

        public async Task<string> GetUrl(
            IDictionary<string, object?> record,
            string filename,
            IDictionary<string, string>? queryParams = null)
        {
            object? collectionId = null, collectionName = null;

            if (string.IsNullOrWhiteSpace(filename) ||
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
                Uri.EscapeDataString(filename)
            );

            // normalize the download query param (same as TS)
            if (queryParams != null &&
                queryParams.TryGetValue("download", out var download) &&
                download == "false")
            {
                queryParams.Remove("download");
            }

            var request = await _http.SendAsync<string>(
                HttpMethod.Get,
                path,
                query: queryParams
            );

            return request ?? string.Empty;
        }


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
