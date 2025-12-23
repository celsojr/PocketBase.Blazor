using System.Net.Http;
using System.Collections.Generic;

namespace PocketBase.Blazor.Options
{
    public class CommonOptions
    {
        public HttpMethod? Method { get; set; }
        public object? Body { get; set; }
        public string? Fields { get; set; }
        public IDictionary<string, string>? Query { get; set; }
    }
}
