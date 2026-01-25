using System.Collections.Generic;

namespace PocketBase.Blazor.Options
{
    public sealed class FileOptions : CommonOptions
    {
        public string? Thumb { get; init; }
        public string? Token { get; init; }
        public string? Download { get; init; }

        public override Dictionary<string, object?> BuildQuery(int page = 1, int perPage = 30)
        {
            var query = base.BuildQuery(page, perPage);

            if (!string.IsNullOrWhiteSpace(Thumb))
                query["thumb"] = Thumb;

            if (!string.IsNullOrWhiteSpace(Token))
                query["token"] = Token;

            if (!string.IsNullOrWhiteSpace(Download))
                query["download"] = Download;

            return query;
        }
    }
}
