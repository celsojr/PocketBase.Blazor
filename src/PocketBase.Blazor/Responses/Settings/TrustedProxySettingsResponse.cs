using System.Collections.Generic;

namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class TrustedProxySettingsResponse
    {
        public List<string> Headers { get; init; } = [];
        public bool UseLeftmostIp { get; init; }
    }
}
