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

        /// <summary>
        /// Gets an expanded record from the Expand dictionary by field name.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the expanded record into.</typeparam>
        /// <param name="fieldName">The name of the expanded field (e.g., "category", "author").</param>
        /// <returns>The deserialized expanded record, or null if not found or deserialization fails.</returns>
        public T? GetExpandedRecord<T>(string fieldName) where T : class
        {
            if (Expand?.TryGetValue(fieldName, out JsonElement? expansion) != true ||
                expansion is not JsonElement jsonElement)
            {
                return null;
            }

            if (jsonElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(
                    jsonElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a list of expanded records from the Expand dictionary by field name.
        /// </summary>
        /// <typeparam name="T">The type to deserialize each expanded record into.</typeparam>
        /// <param name="fieldName">The name of the expanded field (e.g., "tags", "comments").</param>
        /// <returns>A list of deserialized expanded records, or an empty list if not found or deserialization fails.</returns>
        public List<T> GetExpandedList<T>(string fieldName) where T : class
        {
            if (Expand?.TryGetValue(fieldName, out JsonElement? expansion) != true ||
                expansion is not JsonElement jsonElement)
            {
                return new List<T>();
            }

            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                return new List<T>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<T>>(
                    jsonElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<T>();
            }
            catch (JsonException)
            {
                return new List<T>();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
