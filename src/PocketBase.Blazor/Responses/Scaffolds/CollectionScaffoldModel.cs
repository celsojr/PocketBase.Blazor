using System.Collections.Generic;

namespace PocketBase.Blazor.Responses.Scaffolds
{
    public sealed class CollectionScaffoldModel
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public string Type { get; init; } = "";

        public string? ListRule { get; init; }
        public string? ViewRule { get; init; }
        public string? CreateRule { get; init; }
        public string? UpdateRule { get; init; }
        public string? DeleteRule { get; init; }

        public IReadOnlyList<ScaffoldFieldModel> Fields { get; init; } = [];
        public IReadOnlyList<string> Indexes { get; init; } = [];

        public string Created { get; init; } = "";
        public string Updated { get; init; } = "";
        public bool System { get; init; }

        // View collections
        public string? ViewQuery { get; init; }

        // Auth collections
        public string? AuthRule { get; init; }
        public string? ManageRule { get; init; }

        public AuthAlertScaffold? AuthAlert { get; init; }
        public OAuth2Scaffold? OAuth2 { get; init; }
        public PasswordAuthScaffold? PasswordAuth { get; init; }
        public MfaScaffold? Mfa { get; init; }
        public OtpScaffold? Otp { get; init; }

        public TokenConfigScaffold? AuthToken { get; init; }
        public TokenConfigScaffold? PasswordResetToken { get; init; }
        public TokenConfigScaffold? EmailChangeToken { get; init; }
        public TokenConfigScaffold? VerificationToken { get; init; }
        public TokenConfigScaffold? FileToken { get; init; }

        public EmailTemplateScaffold? VerificationTemplate { get; init; }
        public EmailTemplateScaffold? ResetPasswordTemplate { get; init; }
        public EmailTemplateScaffold? ConfirmEmailChangeTemplate { get; init; }
    }
}

