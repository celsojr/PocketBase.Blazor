using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PocketBase.Blazor.Converters;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Models
{
    /// <summary>
    /// Represents a PocketBase collection model.
    /// </summary>
    public class CollectionModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the collection.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp of the collection.
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? Created { get; set; }

        /// <summary>
        /// Gets or sets the last updated timestamp of the collection.
        /// </summary>
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? Updated { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the collection.
        /// </summary>
        public bool? System { get; set; }

        /// <summary>
        /// Gets or sets the type of the collection.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the list access rule of the collection.
        /// </summary>
        public string? ListRule { get; set; }

        /// <summary>
        /// Gets or sets the view access rule of the collection.
        /// </summary>
        public string? ViewRule { get; set; }

        /// <summary>
        /// Gets or sets the create access rule of the collection.
        /// </summary>
        public string? CreateRule { get; set; }

        /// <summary>
        /// Gets or sets the update access rule of the collection.
        /// </summary>
        public string? UpdateRule { get; set; }

        /// <summary>
        /// Gets or sets the delete access rule of the collection.
        /// </summary>
        public string? DeleteRule { get; set; }

        /// <summary>
        /// Gets or sets the options of the collection.
        /// </summary>
        public CollectionOptions? Options { get; set; }

        /// <summary>
        /// Gets or sets the schema fields of the collection.
        /// </summary>
        public IEnumerable<SchemaFieldModel>? Schema { get; set; } = [];
    }
}

