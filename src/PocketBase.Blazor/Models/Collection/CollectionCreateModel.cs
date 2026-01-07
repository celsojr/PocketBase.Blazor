using System.Collections.Generic;
using PocketBase.Blazor.Models.Collection.Fields;

namespace PocketBase.Blazor.Models.Collection
{
    /// <summary>
    /// Base model for creating a new collection.
    /// </summary>
    public abstract class CollectionCreateModel
    {
        /// <summary>
        /// Collection type (e.g. base, auth, etc.).
        /// </summary>
        public string Type { get; init; } = null!;
        /// <summary>
        /// Unique collection name (table name).
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Collection fields.
        /// Required for base/auth collections, optional for views.
        /// </summary>
        public IList<FieldModel> Fields { get; init; } = [];

        /// <summary>
        /// SQL indexes and unique constraints.
        /// Not supported for view collections.
        /// </summary>
        public IList<string> Indexes { get; init; } = [];

        /// <summary>
        /// Marks collection as system (restricted mutations).
        /// </summary>
        public bool? System { get; init; }

        // ---- CRUD rules ----
        public string? ListRule { get; init; }
        public string? ViewRule { get; init; }
        public string? CreateRule { get; init; }
        public string? UpdateRule { get; init; }
        public string? DeleteRule { get; init; }
    }
}

