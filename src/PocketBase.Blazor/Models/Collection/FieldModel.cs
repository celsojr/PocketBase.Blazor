using PocketBase.Blazor.Enums;

namespace PocketBase.Blazor.Models.Collection
{
    public abstract class FieldModel
    {
        public string Name { get; init; } = null!;
        public FieldType Type { get; init; }
        public bool Required { get; init; }
    }
}

