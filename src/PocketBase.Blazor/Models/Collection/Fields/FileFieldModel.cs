using System.Collections.Generic;

namespace PocketBase.Blazor.Models.Collection.Fields
{
    public sealed class FileFieldModel : FieldModel
    {
        public int? MaxSelect { get; init; }
        public int? MaxSize { get; init; }
        public IReadOnlyList<string>? MimeTypes { get; init; }
    }
}

