namespace PocketBase.Blazor.Options
{
    public sealed class MfaOptions
    {
        public bool? Enabled { get; init; }
        public int Duration { get; init; }
        public string? Rule { get; init; }
    }
}

