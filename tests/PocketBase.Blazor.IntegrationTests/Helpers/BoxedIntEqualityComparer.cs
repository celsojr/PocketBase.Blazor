using System.Diagnostics.CodeAnalysis;

namespace PocketBase.Blazor.IntegrationTests.Helpers;

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

