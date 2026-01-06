using PocketBase.Blazor.Models.Collection;

namespace PocketBase.Blazor.Options
{
    public sealed class OtpOptions
    {
        public bool? Enabled { get; init; }
        public int Duration { get; init; }
        public int Length { get; init; }
        public EmailTemplate? EmailTemplate { get; init; }
    }
}

