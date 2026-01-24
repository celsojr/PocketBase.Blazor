namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class SettingsUpdateRequest
    {
        public SmtpSettingsUpdateRequest? Smtp { get; init; }
        public MetaSettingsUpdateRequest? Meta { get; init; }
    }
}

