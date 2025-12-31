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
    }
}

