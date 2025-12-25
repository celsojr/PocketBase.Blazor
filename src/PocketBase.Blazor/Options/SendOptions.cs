using System.Collections.Generic;

namespace PocketBase.Blazor.Options
{
    public abstract class SendOptions
    {
        public Dictionary<string, string>? Headers { get; set; }
        public Dictionary<string, object?>? Query { get; set; }
        public object? Body { get; set; }
        public string? RequestKey { get; set; }
    }
}
