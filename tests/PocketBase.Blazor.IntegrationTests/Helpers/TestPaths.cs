namespace PocketBase.Blazor.IntegrationTests.Helpers;

using static AppContext;
using static Directory;

public static class TestPaths
{
    public static string ProjectDirectory => GetParent(BaseDirectory)!
        .Parent!.Parent!.Parent!.FullName;

    public static string TestDataDirectory => Path.Combine(
        ProjectDirectory, "Data", "pb_data");

    public static string TestMigrationDirectory => Path.Combine(
        ProjectDirectory, "Data", "pb_migrations");

    public static void CleanUpDataDir()
    {
        if (Exists(TestDataDirectory))
        {
            foreach (var file in GetFiles(TestDataDirectory))
            {
                if (Path.GetFileName(file) != ".gitignore")
                    File.Delete(file);
            }
        }

        string[] contained = [
            ".gitignore",
            "1687801000_collections_snapshot.js",
            "1687801090_initial_regular_user.js",
            "1687801090_initial_settings.js",
            "1687801090_initial_superuser.js",
            "1687801099_seed_categories.js",
            "1687801100_seed_posts.js",
        ];

        if (Exists(TestMigrationDirectory))
        {
            foreach (var file in GetFiles(TestMigrationDirectory))
            {
                if (!contained.Contains(file))
                    File.Delete(file);
            }

        }
    }
}

