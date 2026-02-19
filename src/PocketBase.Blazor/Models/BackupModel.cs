using System;
using System.Text.Json.Serialization;

namespace PocketBase.Blazor.Models
{
    public class BackupModel : BaseModel
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("size")]
        public int? Size { get; set; }

        [JsonPropertyName("modified")]
        public DateTime? Modified { get; set; }
    }
}
