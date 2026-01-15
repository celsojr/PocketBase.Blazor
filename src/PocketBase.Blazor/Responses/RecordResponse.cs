using System.Collections.Generic;
using System.Text.Json;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Responses
{
    /// <summary>
    /// Represents a PocketBase record with dynamic field data and optional related record expansions.
    /// </summary>
    public class RecordResponse : BaseModel
    {
        /// <summary>
        /// Gets the expanded related records.
        /// </summary>
        /// <value>
        /// A <see cref="JsonElement"/> containing expanded records loaded via PocketBase's
        /// expand query parameter, or <see langword="null"/> if no expansions were requested.
        /// </value>
        /// <remarks>
        /// This property contains the raw JSON structure of expanded records. Use JSON
        /// deserialization methods to convert this to strongly-typed models when needed.
        /// </remarks>
        public Dictionary<string, JsonElement?>? Expand { get; init; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

