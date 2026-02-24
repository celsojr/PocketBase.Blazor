namespace PocketBase.Blazor.IntegrationTests.Helpers;

public static class AssertExtensions
{
    public static void ShouldBeSuccess<T>(this Result<T> result)
    {
        if (result.IsFailed)
            throw new XunitException(
                string.Join(Environment.NewLine, result.Errors.Select(e => e.Message))
            );
    }

    public static void ShouldHaveProperties(this JsonElement element, params string[] props)
    {
        foreach (string p in props)
        {
            if (!element.TryGetProperty(p, out _))
                throw new XunitException($"Property '{p}' was not found in JSON element.");
        }
    }
}

