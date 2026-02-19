using System;
using System.Runtime.InteropServices;

namespace PocketBase.Blazor.Options
{
    /// <summary>
    /// Options for cron generation.
    /// </summary>
    public sealed class CronGenerationOptions
    {
        /// <summary>
        /// Root Go project directory (where go.mod lives).
        /// </summary>
        public required string ProjectDirectory { get; init; }

        /// <summary>
        /// Directory where cron files will be generated.
        /// </summary>
        public string OutputDirectory { get; } = "cron";

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

        /// <summary>
        /// Module name for the building process
        /// </summary>
        public string ModuleName { get; } = "cron-server";

        /// <summary>
        /// Binary file name that will be used to serve
        /// the Pocketbase web ui and custom crons
        /// </summary>
        public string OutputBinary { get; } = GetBinaryName();

        private static string GetBinaryName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "pb-cron.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "pb-cron-linux";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "pb-cron-macos";

            throw new PlatformNotSupportedException();
        }
    }
}

