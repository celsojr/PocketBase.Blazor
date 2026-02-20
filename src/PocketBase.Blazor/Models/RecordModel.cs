using System.Collections.Generic;
using System.Text.Json;

namespace PocketBase.Blazor.Models
{
    /// <summary>
    /// Represents a PocketBase record with dynamic field data and optional related record expansions.
    /// </summary>
    /// <remarks>
    /// This model is designed to work with PocketBase's dynamic schema where each collection
    /// can have different fields. The <see cref="Data"/> dictionary stores all field values,
    /// while <see cref="Expand"/> contains related records loaded via the `expand` query parameter.
    /// </remarks>
    public class RecordModel : BaseModel
    {
        /// <summary>
        /// Gets or sets the dynamic field data for the record.
        /// </summary>
        /// <value>
        /// A dictionary where keys are field names and values are the corresponding field values.
        /// The dictionary is initialized as empty by default.
        /// </value>
        /// <example>
        /// <code>
        /// var record = new RecordModel();
        /// record.Data["title"] = "Hello World";
        /// record.Data["published"] = true;
        /// record.Data["created"] = DateTime.UtcNow;
        /// </code>
        /// </example>
        public Dictionary<string, object> Data { get; set; } = new();

        /// <summary>
        /// Gets or sets the expanded related records.
        /// </summary>
        /// <value>
        /// A <see cref="JsonElement"/> containing expanded records loaded via PocketBase's
        /// expand query parameter, or <see langword="null"/> if no expansions were requested.
        /// </value>
        /// <remarks>
        /// This property contains the raw JSON structure of expanded records. Use JSON
        /// deserialization methods to convert this to strongly-typed models when needed.
        /// </remarks>
        public JsonElement? Expand { get; set; }
    }
}

