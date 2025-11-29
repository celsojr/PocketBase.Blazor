using System.Collections.Generic;
using System.Threading.Tasks;

namespace PocketBase.Blazor;

public interface IPocketBaseClient
{
    Task<T?> GetRecordAsync<T>(string collection, string id);
    Task<List<T>> GetListAsync<T>(string collection);
    Task<T?> CreateRecordAsync<T>(string collection, object payload);
}
