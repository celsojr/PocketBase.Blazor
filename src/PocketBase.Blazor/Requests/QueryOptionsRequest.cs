using System.Collections.Generic;

namespace PocketBase.Blazor.Requests
{
    public class QueryOptionsRequest
    {
        public string? Filter { get; set; }
        public string? Sort { get; set; }
        public string? Expand { get; set; }
        public int? Page { get; set; }
        public int? PerPage { get; set; }

        public Dictionary<string, string> ToQueryDictionary()
        {
            var dict = new Dictionary<string, string>();
            
            if (!string.IsNullOrWhiteSpace(Filter))
                dict["filter"] = Filter;

            if (!string.IsNullOrWhiteSpace(Sort))
                dict["sort"] = Sort;

            if (!string.IsNullOrWhiteSpace(Expand))
                dict["expand"] = Expand;

            if (Page.HasValue)
                dict["page"] = Page.Value.ToString();

            if (PerPage.HasValue)
                dict["perPage"] = PerPage.Value.ToString();

            return dict;
        }
    }
}
