namespace PocketBase.Blazor.IntegrationTests.Helpers;

public static class TestPaths
{
    public static string ProjectDirectory =>
        Directory.GetParent(AppContext.BaseDirectory)!
        .Parent!.Parent!.Parent!.FullName;

    public static string TestDataDirectory => Path.Combine(
        ProjectDirectory, "Data", "pb_data");

    public static string TestMigrationDirectory => Path.Combine(
        ProjectDirectory, "Data", "pb_migrations");
}

