using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketBase.Blazor.Responses
{
    public class CollectionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("system")]
        public bool System { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("updated")]
        public DateTime Updated { get; set; }

        [JsonPropertyName("fields")]
        public List<Dictionary<string, object>> Fields { get; set; } = new();

        [JsonPropertyName("indexes")]
        public List<string> Indexes { get; set; } = new();

        // Optional: CRUD rules
        [JsonPropertyName("listRule")]
        public string? ListRule { get; set; }

        [JsonPropertyName("viewRule")]
        public string? ViewRule { get; set; }

        [JsonPropertyName("createRule")]
        public string? CreateRule { get; set; }

        [JsonPropertyName("updateRule")]
        public string? UpdateRule { get; set; }

        [JsonPropertyName("deleteRule")]
        public string? DeleteRule { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

