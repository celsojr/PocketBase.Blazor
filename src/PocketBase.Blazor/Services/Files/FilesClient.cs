using System;
using System.IO;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using System.Collections.Generic;

namespace PocketBase.Blazor.Services.Files
{
    public class FilesClient : IFilesClient
    {
        readonly IHttpTransport _http;

        public FilesClient(IHttpTransport http)
        {
            _http = http;
        }

        public Task<Stream> DownloadAsync(string url)
        {
            throw new NotImplementedException();
        }

        public string GetUrl(RecordModel record, string fileName, IDictionary<string, string>? query = null)
        {
            throw new NotImplementedException();
        }
    }
}
