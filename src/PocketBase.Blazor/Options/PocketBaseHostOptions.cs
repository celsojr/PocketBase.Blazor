namespace PocketBase.Blazor.Options
{
    /// <summary>
    /// Options used to configure the PocketBase host process.
    /// </summary>
    public sealed class PocketBaseHostOptions
    {
        /// <summary>Use HTTPS instead of HTTP.</summary>
        public bool UseHttps { get; set; }

        /// <summary>Host address to bind the server to.</summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>Port to listen on.</summary>
        public int Port { get; set; } = 8090;

        /// <summary>
        /// Optional HTTPS port to listen on when <see cref="UseHttps"/> is enabled.
        /// </summary>
        public int? HttpsPort { get; set; }

        /// <summary>Data directory for PocketBase.</summary>
        public string? DataDir { get; set; }

        /// <summary>Enable development mode.</summary>
        public bool Dev { get; set; }

        /// <summary>Directory containing migration files.</summary>
        public string? MigrationsDir { get; set; }

        /// <summary>Optional path to a custom PocketBase executable.</summary>
        public string? Executable { get; set; }
    }
}
