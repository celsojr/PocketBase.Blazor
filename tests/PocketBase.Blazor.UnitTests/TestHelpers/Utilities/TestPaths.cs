namespace PocketBase.Blazor.UnitTests.TestHelpers.Utilities;

public static class TestPaths
{
    public static string ProjectDirectory =>
        Directory.GetParent(AppContext.BaseDirectory)!
        .Parent!.Parent!.Parent!.FullName;

    public static string OutputTestDataDirectory => Path.Combine(
        AppContext.BaseDirectory, "TestData");

    public static string TestDataDirectory => Path.Combine(
        ProjectDirectory, "TestData");
    
    public static string JsonResponsesDirectory => Path.Combine(
        TestDataDirectory, "Json", "Responses");

    public static string LiquidResponsesDirectory => Path.Combine(
        TestDataDirectory, "Liquid");

    public static string PostResponsesDirectory => Path.Combine(
        JsonResponsesDirectory, "PostResponse");
}

