namespace PocketBase.Blazor.Models.Collection.Fields
{
    public sealed class RelationFieldModel : FieldModel
    {
        public string CollectionId { get; init; } = null!;
        public bool CascadeDelete { get; init; }
        public int? MaxSelect { get; init; }
    }
}

