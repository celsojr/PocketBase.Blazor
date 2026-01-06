using System.Collections.Generic;
using PocketBase.Blazor.Models.Collection;

namespace PocketBase.Blazor.Options
{
    public sealed class OAuth2Options
    {
        public bool? Enabled { get; init; }
        public OAuth2MappedFields? MappedFields { get; init; }
        public IList<OAuth2Provider>? Providers { get; init; }
    }
}

