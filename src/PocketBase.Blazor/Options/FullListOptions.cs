namespace PocketBase.Blazor.Options
{
    /// <summary>
    /// Options for retrieving a full list of resources in batches.
    /// </summary>
    public class FullListOptions : ListOptions
    {
        /// <summary>
        /// The number of items to retrieve in each batch.
        /// </summary>
        public int? Batch { get; set; }
    }
}

