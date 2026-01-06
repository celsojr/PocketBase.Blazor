using PocketBase.Blazor.Enums;
using System.Collections.Generic;

namespace PocketBase.Blazor.Models.Collection
{
    public abstract class CollectionCreateModel
    {
        public string Name { get; init; } = null!;
        public CollectionType Type { get; init; }
        public IList<FieldModel> Fields { get; init; } = new List<FieldModel>();
    }
}

