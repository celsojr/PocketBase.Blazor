namespace PocketBase.Blazor.IntegrationTests.Helpers;

using System.Diagnostics.CodeAnalysis;

public class BoxedIntEqualityComparer : IEqualityComparer<int>
{
    public bool Equals(int x, int y)
    {
        return x == y;
    }

    public int GetHashCode([DisallowNull] int obj)
    {
        return obj.GetHashCode();
    }
}

