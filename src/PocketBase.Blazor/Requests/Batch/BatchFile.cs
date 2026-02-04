using System.IO;

namespace PocketBase.Blazor.Requests.Batch
{
    public sealed class BatchFile
    {
        public string Field { get; init; } = null!;
        public Stream Content { get; init; } = null!;
        public string FileName { get; init; } = null!;
        public string? ContentType { get; init; }
    }
}
