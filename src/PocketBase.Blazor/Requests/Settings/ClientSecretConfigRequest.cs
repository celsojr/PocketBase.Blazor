namespace PocketBase.Blazor.Requests.Settings
{
    public class ClientSecretConfigRequest
    {
        public string? KeyId { get; init; }
        public string? ClientId { get; init; }
        public string? TeamId { get; init; }
        public string? PrivateKey { get; init; }
        public int Duration { get; init; }
    }
}

