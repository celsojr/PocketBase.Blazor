using System.Collections.Generic;

namespace PocketBase.Blazor.Events
{
    public sealed class RealtimeRecordEvent
    {
        public string Action { get; init; } = default!;
        public string Collection { get; init; } = default!;
        public string? RecordId { get; init; }
        public Dictionary<string, object?> Record { get; init; } = default!;
    }
}
