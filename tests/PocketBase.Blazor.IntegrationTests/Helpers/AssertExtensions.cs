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
}

