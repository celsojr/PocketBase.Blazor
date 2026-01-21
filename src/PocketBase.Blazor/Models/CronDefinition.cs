using System.Collections.Generic;

namespace PocketBase.Blazor.Models
{
    public sealed class CronDefinition
    {
        /// <summary>
        /// Stable cron identifier (used at runtime).
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Logical handler name (maps to Go function).
        /// </summary>
        public required string Handler { get; init; }

        /// <summary>
        /// Go language function body that executes when the cron triggers.
        /// Example: `log.Println("Hello from cron", payload)`
        /// </summary>
        public string? HandlerBody { get; init; }

        /// <summary>
        /// Additional Go packages to import (e.g., ["fmt", "time", "strings"])
        /// </summary>
        public List<string>? ImportPackages { get; init; }

        /// <summary>
        /// Optional cron description
        /// </summary>
        public string? Description { get; init; }
    }
}

