namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class MetaSettingsResponse
    {
        public string? AppName { get; init; }
        public string? AppUrl { get; init; }
        public string? SenderName { get; init; }
        public string? SenderAddress { get; init; }
        public bool HideControls { get; init; }
    }
}

