using System.Collections.Generic;
using PocketBase.Blazor.Models.Collection.Fields;

namespace PocketBase.Blazor.Models.Collection
{
    public abstract class CollectionCreateModel
    {
        public string Name { get; init; } = null!;
        public string Type { get; init; } = null!;
        public IList<FieldModel> Fields { get; init; } = [];
    }
}

