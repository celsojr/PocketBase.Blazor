using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketBase.Blazor.Responses.Scaffolds
{
    public sealed class ScaffoldFieldModel
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public string Type { get; init; } = "";

        public bool Required { get; init; }
        public bool System { get; init; }
        public bool Hidden { get; init; }
        public bool Presentable { get; init; }

        public bool? PrimaryKey { get; init; }

        // Catch-all for field-specific options:
        // min, max, pattern, cost, providers, etc.
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Options { get; init; } = [];
    }
}

