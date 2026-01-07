using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Models.Collection
{
    public sealed class AuthCollectionCreateModel : CollectionCreateModel
    {
        public string? CreateRule { get; init; }
        public string? UpdateRule { get; init; }
        public string? DeleteRule { get; init; }

        public PasswordAuthOptions PasswordAuth { get; init; } = new();
    }
}

