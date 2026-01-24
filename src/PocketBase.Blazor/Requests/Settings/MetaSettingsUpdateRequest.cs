namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class MetaSettingsUpdateRequest
    {
        public string? AppName { get; set; }
        public string? AppUrl { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAddress { get; set; }
    }
}
