namespace PocketBase.Blazor.Options
{
    public sealed class PocketBaseHostOptions
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8090;
        public string? Dir { get; set; }
        public bool Dev { get; set; }
        public string? MigrationsDir { get; set; }
        public string? Executable { get; set; }
    }
}

