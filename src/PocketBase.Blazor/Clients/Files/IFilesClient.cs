using System.IO;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;
using System.Collections.Generic;

namespace PocketBase.Blazor.Clients.Files
{
    public interface IFilesClient
    {
        string GetUrl(RecordModel record, string fileName, IDictionary<string, string>? query = null);
        Task<Stream> DownloadAsync(string url);
    }
}
