using System.Collections.Generic;

namespace PocketBase.Blazor.Options
{
    public class CommonOptions : SendOptions
    {
        public string? Fields { get; set; }

        public override Dictionary<string, object?> BuildQuery(int page = 1, int perPage = 30)
        {
            if (!string.IsNullOrEmpty(Fields))
            {
                var query = base.BuildQuery(page, perPage);
                query["fields"] = Fields;
                return query;
            }
            return base.BuildQuery(page, perPage);
        }
    }
}
