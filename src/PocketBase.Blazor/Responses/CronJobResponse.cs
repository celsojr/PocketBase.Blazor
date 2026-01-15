using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    public class CronJobResponse
    {
        public string? Id { get; set; }
        public string? Expression { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

