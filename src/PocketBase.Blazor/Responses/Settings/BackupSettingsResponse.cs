namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class BackupSettingsResponse
    {
        public string? Cron { get; init; }
        public int CronMaxKeep { get; init; }
        public S3SettingsResponse? S3 { get; init; }
    }
}
