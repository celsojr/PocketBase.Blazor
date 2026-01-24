using System.Collections.Generic;

namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class RateLimitSettingsResponse
    {
        public List<RateLimitRuleResponse> Rules { get; init; } = [];
        public bool Enabled { get; init; }
    }
}
