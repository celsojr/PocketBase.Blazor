using System;
using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    public sealed class RecordResponse
    {
        public string Id { get; set; } = default!;

        public string CollectionId { get; set; } = default!;

        public string CollectionName { get; set; } = default!;

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public JsonElement Expand { get; set; }

        public JsonElement Record { get; set; }
    }
}
