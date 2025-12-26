using System;
using System.Text.Json;
using System.Collections.Generic;
using PocketBase.Blazor.Enums;

namespace PocketBase.Blazor.Requests
{
    using static BatchMethod;

    public class BatchRequest
    {
        public string? Method { get; set; }
        public string? Url { get; set; }
        public Dictionary<string, JsonElement>? Json { get; set; }
        public Dictionary<string, IEnumerable<JsonElement>>? Files { get; set; }
        public Dictionary<string, string>? Headers { get; set; }

        public BatchRequest(string? collectionName, BatchMethod method, object? body = null, string? id = null)
        {
            Method = method switch
            {
                Create or Upsert => "POST",
                Update => "PATCH",
                Delete => "DELETE",
                _ => throw new ArgumentException("Invalid batch method", nameof(method)),
            };

            Url = method switch
            {
                Create or Upsert
                    => $"api/collections/{collectionName}/records",

                Update or Delete
                    => id is not null
                        ? $"api/collections/{collectionName}/records/{id}"
                        : throw new ArgumentNullException(nameof(id)),

                _ => throw new ArgumentException("Invalid batch method", nameof(method)),
            };

            if (body != null)
            {
                var jsonString = JsonSerializer.Serialize(body);
                var jsonDoc = JsonDocument.Parse(jsonString);
                Json = new Dictionary<string, JsonElement>
                {
                    { "data", jsonDoc.RootElement.Clone() }
                };
            }
        }
    }
}

