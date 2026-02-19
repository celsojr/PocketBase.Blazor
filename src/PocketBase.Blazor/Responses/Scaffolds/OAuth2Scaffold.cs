using System.Collections.Generic;

namespace PocketBase.Blazor.Responses.Scaffolds
{
    public sealed class OAuth2Scaffold
    {
        public bool Enabled { get; init; }
        public IReadOnlyList<object> Providers { get; init; } = [];
        public OAuth2MappedFieldsScaffold? MappedFields { get; init; }
    }
}

