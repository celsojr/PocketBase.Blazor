using System.Text.Json.Serialization;

namespace PocketBase.Blazor.Models.Collection.Fields
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(TextFieldModel), "text")]
    [JsonDerivedType(typeof(BoolFieldModel), "bool")]
    [JsonDerivedType(typeof(NumberFieldModel), "number")]
    [JsonDerivedType(typeof(EmailFieldModel), "email")]
    [JsonDerivedType(typeof(UrlFieldModel), "url")]
    [JsonDerivedType(typeof(JsonFieldModel), "json")]
    [JsonDerivedType(typeof(DateFieldModel), "date")]
    [JsonDerivedType(typeof(RelationFieldModel), "relation")]
    [JsonDerivedType(typeof(FileFieldModel), "file")]
    public abstract class FieldModel
    {
        public string Name { get; init; } = null!;
        public bool Required { get; init; }
        public bool Unique { get; init; }
        public bool System { get; init; }
    }
}

