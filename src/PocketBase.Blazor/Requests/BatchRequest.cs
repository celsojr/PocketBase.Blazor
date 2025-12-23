using System.Text.Json;
using System.Collections.Generic;

namespace PocketBase.Blazor.Requests
{
    public class BatchRequest
    {
        public string? Method { get; set; }
        public string? Url { get; set; }
        public Dictionary<string, JsonElement>? Json { get; set; }
        public Dictionary<string, IEnumerable<JsonElement>>? Files { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}

