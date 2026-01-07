using System.Text.Json.Serialization;

namespace PocketBase.Blazor.Models.Collection
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(TextFieldModel), "text")]
    [JsonDerivedType(typeof(BoolFieldModel), "bool")]
    public abstract class FieldModel
    {
        public string Name { get; init; } = null!;
        public bool Required { get; init; }
    }
}

