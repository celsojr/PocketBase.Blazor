namespace PocketBase.Blazor.Models.Collection.Fields
{
    public sealed class TextFieldModel : FieldModel
    {
        public int? Min { get; init; }
        public int? Max { get; init; }
        public string? Pattern { get; init; }
    }
}

