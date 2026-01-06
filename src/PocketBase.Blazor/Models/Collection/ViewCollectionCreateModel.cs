using PocketBase.Blazor.Enums;

namespace PocketBase.Blazor.Models.Collection
{
    public sealed class ViewCollectionCreateModel : CollectionCreateModel
    {
        public string? ListRule { get; init; }
        public string? ViewRule { get; init; }
        public string ViewQuery { get; init; } = null!;

        public ViewCollectionCreateModel()
        {
            Type = nameof(CollectionType.View).ToLowerInvariant();
        }
    }
}

