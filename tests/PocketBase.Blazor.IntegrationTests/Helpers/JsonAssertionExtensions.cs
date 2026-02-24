namespace PocketBase.Blazor.IntegrationTests.Helpers;

public static class JsonAssertionExtensions
{
    public static Dictionary<string, object> ShouldBeJsonObject(this object item)
    {
        ArgumentNullException.ThrowIfNull(item);

        string jsonText = item.ToString() ?? "{}";
        Dictionary<string, object> dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonText)!;

        dict.Should().NotBeNull("The object should deserialize to a JSON dictionary");
        return dict;
    }

    public static Dictionary<string, object> HaveProperty(this Dictionary<string, object> dict, string propertyName)
    {
        dict.Should().ContainKey(propertyName, $"Expected JSON object to have property '{propertyName}'");
        return dict;
    }
}

