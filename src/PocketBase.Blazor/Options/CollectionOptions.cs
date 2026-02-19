namespace PocketBase.Blazor.Options
{
    /// <summary>
    /// Represents the options for a PocketBase collection.
    /// </summary>
    public class CollectionOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether email authentication is allowed.
        /// </summary>
        public bool? AllowEmailAuth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether OAuth2 authentication is allowed.
        /// </summary>
        public bool? AllowOAuth2Auth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether username/password authentication is allowed.
        /// </summary>
        public bool? AllowUsernameAuth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an email is required for user registration.
        /// </summary>
        public bool? RequireEmail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a username is required for user registration.
        /// </summary>
        public int? MinPasswordLength { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of email domains that are excluded from registration.
        /// </summary>
        public string? ExceptEmailDomains { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of email domains that are required for registration.
        /// </summary>
        public string? OnlyEmailDomains { get; set; }

        /// <summary>
        /// Gets or sets the management access rule of the collection.
        /// </summary>
        public string? ManageRule { get; set; }
    }
}

