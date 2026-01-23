using System.Collections.Generic;

namespace PocketBase.Blazor.Models
{
    /// <summary>
    /// Defines a cron job for PocketBase with its handler and metadata.
    /// </summary>
    public sealed class CronDefinition
    {
        /// <summary>
        /// Stable cron identifier (used at runtime).
        /// Maps to the custom cron ID in PocketBase endpoint.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Optional cron description
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Go language function body that executes when the cron triggers.
        /// Example: `log.Println("Hello from cron", payload)`
        /// </summary>
        public required string? HandlerBody { get; init; }

        /// <summary>
        /// Additional Go packages to import (e.g., ["fmt", "time", "strings"])
        /// </summary>
        public List<string>? ImportPackages { get; init; }
    }
}

