using System.Collections.Generic;

namespace PocketBase.Blazor.Requests
{
    public sealed class CronRegisterRequest
    {
        public required string Id { get; init; }
        public required string Expression { get; init; }
        public required Dictionary<string, object?> Payload { get; init; }
    }
}

