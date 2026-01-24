namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class RateLimitRuleResponse
    {
        public string? Label { get; init; }
        public string? Audience { get; init; }
        public int Duration { get; init; }
        public int MaxRequests { get; init; }
    }
}
