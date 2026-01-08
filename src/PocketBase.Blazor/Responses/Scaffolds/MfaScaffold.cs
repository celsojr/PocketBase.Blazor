namespace PocketBase.Blazor.Responses.Scaffolds
{
    public sealed class MfaScaffold
    {
        public bool Enabled { get; init; }
        public int Duration { get; init; }
        public string? Rule { get; init; }
    }
}

