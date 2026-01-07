using System.Collections.Generic;
using System.Text.Json;

namespace PocketBase.Blazor.Models
{
    public class RecordModel : BaseModel
    {
        // dynamic fields
        public Dictionary<string, object> Data { get; set; } = new();

        public JsonElement? Expand { get; set; }
    }
}

