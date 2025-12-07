using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PocketBase.Blazor.Http
{
    public interface IHttpTransport
    {
        Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null, IDictionary<string, string>? query = null);
        Task SendAsync(HttpMethod method, string path, object? body = null, IDictionary<string, string>? query = null);
    }
}
