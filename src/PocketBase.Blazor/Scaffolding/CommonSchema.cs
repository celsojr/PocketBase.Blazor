namespace PocketBase.Blazor.Scaffolding
{
    /// <summary>
    /// Represents built-in schema templates that can be scaffolded as PocketBase migration files.
    /// </summary>
    public enum CommonSchema
    {
        /// <summary>
        /// Blog-like schema with categories and posts collections.
        /// </summary>
        Blog = 0,

        /// <summary>
        /// Todo-like schema with a single todos collection.
        /// </summary>
        Todo = 1,

        /// <summary>
        /// E-commerce-like schema with products and orders collections.
        /// </summary>
        ECommerce = 2
    }
}
