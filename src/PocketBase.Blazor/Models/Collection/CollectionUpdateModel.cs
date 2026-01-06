using System.Collections.Generic;

namespace PocketBase.Blazor.Models.Collection
{
    public abstract class CollectionUpdateModel
    {
        /// <summary>
        /// Unique collection name (table name).
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Collection fields.
        /// Required for base/auth collections, optional for views.
        /// </summary>
        public IList<FieldModel>? Fields { get; init; }

        /// <summary>
        /// SQL indexes and unique constraints.
        /// Not supported for view collections.
        /// </summary>
        public IList<string>? Indexes { get; init; }

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

