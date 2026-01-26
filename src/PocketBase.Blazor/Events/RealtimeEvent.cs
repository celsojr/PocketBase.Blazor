namespace PocketBase.Blazor.Events
{
    public sealed class RealtimeEvent
    {
        public string? Id { get; init; }
        public string Event { get; init; } = default!;
        public string Data { get; init; } = default!;
        public string? Topic { get; internal set; }
    }
}
