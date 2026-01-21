using System.Collections.Generic;

namespace PocketBase.Blazor.Models
{
    public sealed class CronManifest
    {
        public List<CronDefinition> Crons { get; init; } = [];
    }
}

/*
var manifest = new CronManifest
{
    Crons =
    {
        new()
        {
            Id = "hello",
            Description = "Simple hello logger",
            Handler = "hello"
        },
        new()
        {
            Id = "reindex",
            Description = "Reindex a collection",
            Handler = "reindex"
        }
    }
};

var json = JsonSerializer.Serialize(
    manifest,
    new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    });

File.WriteAllText("crons.json", json);

*/
