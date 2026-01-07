using System.Collections.Generic;

namespace PocketBase.Blazor.Models.Collection.Fields
{
    public sealed class EmailFieldModel : FieldModel
    {
        public IReadOnlyList<string>? ExceptDomains { get; init; }
        public IReadOnlyList<string>? OnlyDomains { get; init; }
    }
}

