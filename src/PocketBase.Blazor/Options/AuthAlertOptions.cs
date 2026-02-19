using PocketBase.Blazor.Models.Collection;

namespace PocketBase.Blazor.Options
{
    public sealed class AuthAlertOptions
    {
        public bool? Enabled { get; init; }
        public EmailTemplate? EmailTemplate { get; init; }
    }
}

