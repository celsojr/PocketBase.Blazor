using System.Text.Json;

namespace PocketBase.Blazor.Responses.Cron
{
    /// <summary>
    /// Represents a cron job configuration response.
    /// </summary>
    public class CronsResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the cron job.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the cron expression defining when the job should run.
        /// </summary>
        public string? Expression { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

