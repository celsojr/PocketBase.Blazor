using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    public class LogResponse
    {
        public string? Level { get; set; }
        public string? Message { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
        public Dictionary<string, JsonElement>? Data { get; set; }
    }
}

