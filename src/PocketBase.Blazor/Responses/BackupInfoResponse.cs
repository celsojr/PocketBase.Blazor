using System;
using System.Text.Json;

namespace PocketBase.Blazor.Responses
{
    /// <summary>
    /// Backup information response.
    /// </summary>
    public class BackupInfoResponse
    {
        /// <summary>
        /// The key (filename) of the backup.
        /// </summary>
        public string Key { get; set; } = "";

        /// <summary>
        /// Last modified timestamp (string or ISO-format).
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

