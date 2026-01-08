using System;
using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    public class RecordResponse
    {
        public string Id { get; set; } = null!;

        public string CollectionId { get; set; } = null!;

        public string CollectionName { get; set; } = null!;

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public JsonElement Expand { get; set; }
    }
}

