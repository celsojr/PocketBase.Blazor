using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<Result<byte[]>> GetUrl(string collectionId, string recordId, string fileName, IDictionary<string, object?>? query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName) ||
                string.IsNullOrWhiteSpace(recordId))
            {
                return Array.Empty<byte>();
            }

            var path = string.Join('/', [
                "api",
                "files",
                Uri.EscapeDataString(collectionId),
                Uri.EscapeDataString(recordId),
                Uri.EscapeDataString(fileName)
            ]);

            // normalize the download query param (same as TS)
            if (query != null &&
                query.TryGetValue("download", out var download) &&
                download?.Equals("false") == true)
            {
                query.Remove("download");
            }

            var request = await _http.SendAsync<byte[]>(
                HttpMethod.Get,
                path,
                query: query,
                cancellationToken: cancellationToken
            );

            return request;
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

        // ===

        //public class FileOptions
        //{
        //    public string? Thumb { get; set; }
        //    public string? Token { get; set; }
        //    public bool Download { get; set; }
        //}

        //public async Task<Result<byte[]>> GetFileAsync(string collectionId, string recordId, string fileName, FileOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(recordId))
        //    {
        //        return Result.Fail<byte[]>("FileName and RecordId are required");
        //    }

        //    var path = string.Join('/', [
        //        "api",
        //        "files",
        //        Uri.EscapeDataString(collectionId),
        //        Uri.EscapeDataString(recordId),
        //        Uri.EscapeDataString(fileName)
        //    ]);

        //    var query = new Dictionary<string, object?>();
    
        //    if (options != null)
        //    {
        //        if (!string.IsNullOrWhiteSpace(options.Thumb))
        //            query["thumb"] = options.Thumb;
        
        //        if (!string.IsNullOrWhiteSpace(options.Token))
        //            query["token"] = options.Token;
        
        //        if (options.Download)
        //            query["download"] = "1";
        //    }

        //    return await _http.SendRawAsync(HttpMethod.Get, path, query: query, cancellationToken: cancellationToken);
        //}

        //// For URL only (no download):
        //public string GetFileUrl(string collectionId, string recordId, string fileName, FileOptions? options = null)
        //{
        //    var path = string.Join('/', [
        //        "api",
        //        "files",
        //        Uri.EscapeDataString(collectionId),
        //        Uri.EscapeDataString(recordId),
        //        Uri.EscapeDataString(fileName)
        //    ]);

        //    var query = new Dictionary<string, object?>();
    
        //    if (options != null)
        //    {
        //        if (!string.IsNullOrWhiteSpace(options.Thumb))
        //            query["thumb"] = options.Thumb;
        
        //        if (!string.IsNullOrWhiteSpace(options.Token))
        //            query["token"] = options.Token;
        
        //        if (options.Download)
        //            query["download"] = "1";
        //    }

        //    var queryString = query.Any() 
        //        ? "?" + string.Join("&", query.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"))
        //        : "";

        //    return path + queryString;
        //}
    }
}

