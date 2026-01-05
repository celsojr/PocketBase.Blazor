using System.Collections.Generic;

namespace PocketBase.Blazor.Options
{
    /// <summary>
    /// Options for listing resources with pagination, sorting, and filtering.
    /// </summary>
    public class ListOptions : CommonOptions
    {
        /// <summary>
        /// The page number to retrieve (1-based).
        /// </summary>
        public int? Page { get; set; }

        /// <summary>
        /// The number of items to retrieve per page.
        /// </summary>
        public int? PerPage { get; set; }

        /// <summary>
        /// The field to sort the results by.
        /// </summary>
        public string? Sort { get; set; }

        /// <summary>
        /// The field to filter the results by.
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// If true, skips the total count calculation for performance optimization.
        /// </summary>
        public bool SkipTotal { get; set; }

        internal Dictionary<string, object?> BuildQuery(int page = 1, int perPage = 30)
        {
            var query = base.BuildQuery(page, perPage);

            // PocketBase defaults are page=1 and perPage=30
            query["page"] = page != 1 ? page : Page;
            query["perPage"] = perPage != 30 ? perPage : PerPage;

            if (!string.IsNullOrEmpty(Sort))
                query["sort"] = Sort;

            if (!string.IsNullOrEmpty(Filter))
                query["filter"] = Filter;

            if (SkipTotal)
                query["skipTotal"] = "true";

            return query;
        }
    }
}

