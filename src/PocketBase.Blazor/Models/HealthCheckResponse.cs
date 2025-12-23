using System.Text.Json;

namespace PocketBase.Blazor.Models
{
    public class HealthCheckResponse
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public JsonElement? Data { get; set; }
    }
}

