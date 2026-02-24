using System.Collections.Generic;
using PocketBase.Blazor.Requests;

namespace PocketBase.Blazor.Extensions
{
    public static class QueryOptionsExtensions
    {
        public static Dictionary<string, object?> ToQueryDictionary(this QueryOptionsRequest? options)
        {
            Dictionary<string, object?> dict = new Dictionary<string, object?>();

            if (options == null)
                return dict;

            if (!string.IsNullOrWhiteSpace(options.Filter))
                dict["filter"] = options.Filter;

            if (!string.IsNullOrWhiteSpace(options.Sort))
                dict["sort"] = options.Sort;

            if (!string.IsNullOrWhiteSpace(options.Expand))
                dict["expand"] = options.Expand;

            if (options.Page.HasValue)
                dict["page"] = options.Page.Value;

            if (options.PerPage.HasValue)
                dict["perPage"] = options.PerPage.Value;

            return dict;
        }
    }
}

