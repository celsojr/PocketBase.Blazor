namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class BatchSettingsResponse
    {
        public bool Enabled { get; init; }
        public int MaxRequests { get; init; }
        public int Timeout { get; init; }
        public int MaxBodySize { get; init; }
    }
}
