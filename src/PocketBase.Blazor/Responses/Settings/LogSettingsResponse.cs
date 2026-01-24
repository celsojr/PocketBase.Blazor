namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class LogSettingsResponse
    {
        public int MaxDays { get; init; }
        public int MinLevel { get; init; }
        public bool LogIp { get; init; }
        public bool LogAuthId { get; init; }
    }
}
