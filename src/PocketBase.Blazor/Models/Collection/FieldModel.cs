namespace PocketBase.Blazor.Models.Collection
{
    public abstract class FieldModel
    {
        public string Name { get; init; } = null!;
        public string Type { get; init; } = null!;
        public bool Required { get; init; }
    }
}

