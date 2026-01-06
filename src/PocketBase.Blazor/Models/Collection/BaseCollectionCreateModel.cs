using PocketBase.Blazor.Enums;

namespace PocketBase.Blazor.Models.Collection
{
    public sealed class BaseCollectionCreateModel : CollectionCreateModel
    {
        public BaseCollectionCreateModel()
        {
            Type = CollectionType.Base;
        }
    }
}

