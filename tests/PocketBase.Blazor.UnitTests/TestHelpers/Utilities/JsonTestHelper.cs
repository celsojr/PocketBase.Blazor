namespace PocketBase.Blazor.UnitTests.TestHelpers.Utilities;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonTestHelper
{
    public static JsonSerializerOptions DefaultOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static JsonSerializerOptions SnakeCaseOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
        WriteIndented = true
    };

    public static async Task<T> DeserializeFromFileAsync<T>(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json, DefaultOptions)!;
    }

    public static string SerializeWithSettings(object obj, bool indented = true)
    {
        var options = new JsonSerializerOptions(DefaultOptions)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(obj, options);
    }

    public static bool JsonEquals(object expected, object actual)
    {
        var expectedJson = JsonSerializer.Serialize(expected, DefaultOptions);
        var actualJson = JsonSerializer.Serialize(actual, DefaultOptions);
        return expectedJson == actualJson;
    }

    public static class TestFiles
    {
        public const string CompletePost = "TestData/Json/Responses/PostResponse/CompletePost.json";
        public const string MinimalPost = "TestData/Json/Responses/PostResponse/MinimalPost.json";
        public const string WithExpand = "TestData/Json/Responses/PostResponse/WithExpand.json";
    }
}

public class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var result = new StringBuilder();
        result.Append(char.ToLowerInvariant(name[0]));

        for (int i = 1; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(name[i]));
            }
            else
            {
                result.Append(name[i]);
            }
        }

        return result.ToString();
    }
}

