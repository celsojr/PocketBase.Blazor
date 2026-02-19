namespace PocketBase.Blazor.Models.Collection
{
    public sealed class ViewCollectionUpdateModel : CollectionUpdateModel
    {
        /// <summary>
        /// SQL query defining the view.
        /// Required for view collections.
        /// </summary>
        public string? ViewQuery { get; init; }
    }
}

