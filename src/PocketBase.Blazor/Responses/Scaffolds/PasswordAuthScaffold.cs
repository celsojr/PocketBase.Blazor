using System.Collections.Generic;

namespace PocketBase.Blazor.Responses.Scaffolds
{
    public sealed class PasswordAuthScaffold
    {
        public bool Enabled { get; init; }
        public IReadOnlyList<string> IdentityFields { get; init; } = [];
    }
}

