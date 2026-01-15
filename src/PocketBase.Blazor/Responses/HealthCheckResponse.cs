using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    /// <summary>
    /// Health check response.
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Status code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Additional data.
        /// </summary>
        public JsonElement? Data { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

