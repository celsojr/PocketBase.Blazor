using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    public class RecordResponse
    {
        public string Id { get; init; } = null!;

        public string CollectionId { get; init; } = null!;

        public string CollectionName { get; init; } = null!;

        public DateTime Created { get; init; }

        public DateTime Updated { get; init; }

        public Dictionary<string, JsonElement?> Expand { get; init; }
    }
}

