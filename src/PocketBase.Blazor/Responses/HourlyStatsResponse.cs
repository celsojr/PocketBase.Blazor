using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    public sealed class HourlyStatsResponse
    {
        public int Total { get; set; }
        public string? Date { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
