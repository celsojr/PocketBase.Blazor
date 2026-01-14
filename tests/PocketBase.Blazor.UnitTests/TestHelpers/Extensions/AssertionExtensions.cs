namespace PocketBase.Blazor.UnitTests.TestHelpers.Extensions;

using System.Text.Json;
using Blazor.Models;
using UnitTests.TestHelpers.Utilities;

public static class AssertionExtensions
{
    public static void ShouldBeEquivalentToJson(this object actual, object expected)
    {
        var actualJson = JsonSerializer.Serialize(actual, JsonTestHelper.DefaultOptions);
        var expectedJson = JsonSerializer.Serialize(expected, JsonTestHelper.DefaultOptions);

        Assert.Equal(expectedJson, actualJson);
    }

    public static void ShouldDeserializeFromJson<T>(this string json)
    {
        var result = JsonSerializer.Deserialize<T>(json, JsonTestHelper.DefaultOptions);
        Assert.NotNull(result);
    }

    public static void ShouldHaveJsonProperty<T>(this T obj, string propertyName, object expectedValue)
    {
        var json = JsonSerializer.Serialize(obj, JsonTestHelper.DefaultOptions);
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty(propertyName, out var property));

        var actualValue = property.Deserialize(expectedValue.GetType());
        Assert.Equal(expectedValue, actualValue);
    }

    public static void ShouldHaveValidId(this BaseModel model)
    {
        Assert.NotNull(model.Id);
        Assert.NotEmpty(model.Id);
        Assert.True(Guid.TryParse(model.Id, out _) || model.Id.Length > 5);
    }

    public static void ShouldHaveValidTimestamps(this BaseModel model)
    {
        if (model.Created.HasValue)
        {
            Assert.True(model.Created.Value <= DateTime.UtcNow);
        }

        if (model.Updated.HasValue)
        {
            Assert.True(model.Updated.Value <= DateTime.UtcNow);
            if (model.Created.HasValue)
            {
                Assert.True(model.Updated.Value >= model.Created.Value);
            }
        }
    }
}

