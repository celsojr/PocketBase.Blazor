namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class SmtpSettingsResponse
    {
        public bool Enabled { get; init; }
        public int Port { get; init; }
        public string? Host { get; init; }
        public string? Username { get; init; }
        public string? AuthMethod { get; init; }
        public bool Tls { get; init; }
        public string? LocalName { get; init; }
    }
}

