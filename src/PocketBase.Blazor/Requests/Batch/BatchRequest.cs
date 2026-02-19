using System;
using System.Text.Json;
using System.Collections.Generic;
using PocketBase.Blazor.Enums;

namespace PocketBase.Blazor.Requests.Batch
{
    using static BatchMethod;

    public sealed class BatchRequest
    {
        // Compile-time constants for HTTP methods
        private const string POST = nameof(POST);
        private const string PATCH = nameof(PATCH);
        private const string DELETE = nameof(DELETE);

        public string Method { get; }
        public string Url { get; }
        public JsonElement? Body { get; }
        public List<BatchFile>? Files { get; private set; }

        public BatchRequest(string collectionName, BatchMethod method, object? body = null, string? id = null, List<BatchFile>? files = null)
        {
            Method = method switch
            {
                Create or Upsert => POST,
                Update => PATCH,
                Delete => DELETE,
                _ => throw new ArgumentException(nameof(method))
            };

            Url = method switch
            {
                Create or Upsert
                    => $"/api/collections/{collectionName}/records",

                Update or Delete
                    => id is not null
                        ? $"/api/collections/{collectionName}/records/{id}"
                        : throw new ArgumentNullException(nameof(id)),

                _ => throw new ArgumentException(nameof(method))
            };

            if (body is not null)
                Body = JsonSerializer.SerializeToElement(body);

            Files = files;
        }

        /// <summary>
        /// Attach a file to this batch request.
        /// </summary>
        public BatchRequest Attach(string field, BatchFile file)
        {
            Files ??= new List<BatchFile>();
            Files.Add(BatchFile.FromStream(file.Content, field, file.FileName, file.ContentType));
            return this;
        }
    }

}

