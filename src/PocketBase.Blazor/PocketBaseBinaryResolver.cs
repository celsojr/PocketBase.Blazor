using System.Runtime.InteropServices;

namespace PocketBase.Blazor
{
    public static class PocketBaseBinaryResolver
    {
        public static string Resolve()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "pb-server.exe";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "pb-server-macos";

            return "pb-server-linux";
        }
    }
}

