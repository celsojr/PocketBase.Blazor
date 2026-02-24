using System.Collections.Generic;

namespace PocketBase.Blazor.Options
{
    public class CommonOptions : SendOptions
    {
        public string? Fields { get; set; }
        public string? Expand { get; set; }

        public override Dictionary<string, object?> BuildQuery(int page = 1, int perPage = 30)
        {
            Dictionary<string, object?> query = base.BuildQuery(page, perPage);

            if (!string.IsNullOrEmpty(Fields))
            {
                query["fields"] = Fields;
            }

            if (!string.IsNullOrEmpty(Expand))
            {
                query["expand"] = Expand;
            }

            return query;
        }
    }
}
