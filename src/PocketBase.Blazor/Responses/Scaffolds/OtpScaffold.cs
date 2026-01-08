namespace PocketBase.Blazor.Responses.Scaffolds
{
    public sealed class OtpScaffold
    {
        public bool Enabled { get; init; }
        public int Duration { get; init; }
        public int Length { get; init; }
        public EmailTemplateScaffold? EmailTemplate { get; init; }
    }
}

