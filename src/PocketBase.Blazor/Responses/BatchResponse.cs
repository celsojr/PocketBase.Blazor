using System.Collections.Generic;
using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    public class BatchResponse
    {
        public int Status { get; set; }
        public Dictionary<string, object>? Body { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

