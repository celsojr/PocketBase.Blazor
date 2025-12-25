using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PocketBase.Blazor.Converters;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Models
{
    public class CollectionModel
    {
        public string? Id { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? Created { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? Updated { get; set; }

        public string? Name { get; set; }
        public bool? System { get; set; }
        public string? Type { get; set; }

        public string? ListRule { get; set; }
        public string? ViewRule { get; set; }
        public string? CreateRule { get; set; }
        public string? UpdateRule { get; set; }
        public string? DeleteRule { get; set; }

        public CollectionOptions? Options { get; set; }
        public IEnumerable<SchemaFieldModel>? Schema { get; set; } = [];
    }
}

