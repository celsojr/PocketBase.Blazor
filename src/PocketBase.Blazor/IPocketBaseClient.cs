using System.Collections.Generic;
using System.Threading.Tasks;

namespace PocketBase.Blazor;

public interface IPocketBaseClient
{
    Task<(T? Value, PocketBaseError? Error)> GetRecordAsync<T>(string collection, string id);

    Task<(List<T> Items, PocketBaseError? Error)> GetListAsync<T>(string collection);

    Task<(T? Value, PocketBaseError? Error)> CreateRecordAsync<T>(string collection, object payload);
}
