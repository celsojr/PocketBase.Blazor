namespace PocketBase.Blazor.Options
{
    public sealed class CronGenerationOptions
    {
        /// <summary>
        /// Root Go project directory (where go.mod lives).
        /// </summary>
        public required string ProjectDirectory { get; init; }

        /// <summary>
        /// Directory where cron files will be generated.
        /// </summary>
        public string OutputDirectory { get; init; } = "internal/crons";

        /// <summary>
        /// Whether to run `go build` after generation.
        /// </summary>
        public bool BuildBinary { get; init; } = true;

        /// <summary>
        /// Optional path to Go executable.
        /// </summary>
        public string GoExecutable { get; init; } = "go";

        /// <summary>
        /// Clean output directory before generation.
        /// </summary>
        public bool CleanBeforeGenerate { get; init; } = true;
    }
}

