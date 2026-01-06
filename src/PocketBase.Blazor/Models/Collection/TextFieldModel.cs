using PocketBase.Blazor.Enums;

namespace PocketBase.Blazor.Models.Collection
{
    public sealed class TextFieldModel : FieldModel
    {
        public int? Min { get; init; }
        public int? Max { get; init; }

        public TextFieldModel()
        {
            Type = FieldType.Text;
        }
    }
}

