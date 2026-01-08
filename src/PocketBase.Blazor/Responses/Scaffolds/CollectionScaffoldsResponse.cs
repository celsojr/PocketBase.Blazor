namespace PocketBase.Blazor.Responses.Scaffolds
{
    public sealed class CollectionScaffoldsResponse
    {
        public CollectionScaffoldModel Auth { get; init; } = null!;
        public CollectionScaffoldModel Base { get; init; } = null!;
        public CollectionScaffoldModel View { get; init; } = null!;
    }
}

