namespace PocketBase.Blazor.Models.Collection
{
    public sealed class OAuth2Provider
    {
        public string Name { get; init; } = null!;
        public string ClientId { get; init; } = null!;
        public string ClientSecret { get; init; } = null!;
        public string? AuthURL { get; init; }
        public string? TokenURL { get; init; }
        public string? UserInfoURL { get; init; }
        public string? DisplayName { get; init; }
        public bool? Pkce { get; init; }
        public object? Extra { get; init; }
    }
}

