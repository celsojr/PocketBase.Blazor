namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class S3SettingsResponse
    {
        public bool Enabled { get; init; }
        public string? Bucket { get; init; }
        public string? Region { get; init; }
        public string? Endpoint { get; init; }
        public string? AccessKey { get; init; }
        public bool ForcePathStyle { get; init; }
    }
}
