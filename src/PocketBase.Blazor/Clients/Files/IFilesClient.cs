using System.Threading.Tasks;
using PocketBase.Blazor.Models;
using System.Collections.Generic;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Files
{
    public interface IFilesClient
    {
        string GetUrl(RecordModel record, string fileName, IDictionary<string, string>? query = null);
        Task<string> GetTokenAsync(CommonOptions? options = null);
    }
}
