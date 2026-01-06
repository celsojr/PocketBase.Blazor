using PocketBase.Blazor.Enums;

namespace PocketBase.Blazor.Models.Collection
{
    public sealed class BoolFieldModel : FieldModel
    {
        public BoolFieldModel()
        {
            Type = nameof(FieldType.Bool).ToLowerInvariant();
        }
    }
}

