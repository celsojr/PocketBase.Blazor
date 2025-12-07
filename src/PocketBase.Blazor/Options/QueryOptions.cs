using System.Collections.Generic;

namespace PocketBase.Blazor.Options
{
    public class QueryOptions
    {
        public Dictionary<string, string> Values { get; } = new();

        public IDictionary<string, string> ToQuery()
            => Values;

        public QueryOptions WithFilter(string filter)
        {
            Values["filter"] = filter;
            return this;
        }

        public QueryOptions WithPagination(int page, int perPage)
        {
            Values["page"] = page.ToString();
            Values["perPage"] = perPage.ToString();
            return this;
        }
    }
}
