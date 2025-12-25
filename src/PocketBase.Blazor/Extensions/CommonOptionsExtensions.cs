using System.Collections.Generic;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="CommonOptions"/>.
    /// </summary>
    public static class CommonOptionsExtensions
    {
        /// <summary>
        /// Converts a <see cref="CommonOptions"/> instance into a dictionary suitable for query parameters.
        /// </summary>
        public static IDictionary<string, object?> ToDictionary(this CommonOptions options)
        {
            var dict = options?.Query != null
                ? new Dictionary<string, object?>(options.Query)
                : [];

            if (options != null && !string.IsNullOrWhiteSpace(options.Fields))
            {
                dict["fields"] = options.Fields!;
            }

            return dict;
        }
    }
}
