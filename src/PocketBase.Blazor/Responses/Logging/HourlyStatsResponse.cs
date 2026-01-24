using System;
using System.Text.Json;

namespace PocketBase.Blazor.Responses.Logging
{
    public sealed class HourlyStatsResponse
    {
        public int Total { get; init; }
        public DateTime? Date { get; init; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
