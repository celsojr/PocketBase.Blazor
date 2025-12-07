using System;
using System.Text.Json;

namespace PocketBase.Blazor.Models;

public class RecordModel
{
    public string Id { get; set; } = "";
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    // dynamic fields
    public JsonElement Data { get; set; }

    public JsonElement? Expand { get; set; }
}
