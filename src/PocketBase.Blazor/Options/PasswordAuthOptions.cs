using System.Collections.Generic;

namespace PocketBase.Blazor.Options
{

    public sealed class PasswordAuthOptions
    {
        public bool Enabled { get; init; }
        public IList<string> IdentityFields { get; init; } = new List<string>();
    }
}

