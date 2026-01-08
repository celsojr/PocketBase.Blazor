namespace PocketBase.Blazor.Responses.Scaffolds
{
    public sealed class AuthAlertScaffold
    {
        public bool Enabled { get; init; }
        public EmailTemplateScaffold? EmailTemplate { get; init; }
    }
}

