using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PocketBase.Blazor.Models;

public class RecordModel
{
    public string Id { get; set; } = "";
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    // dynamic fields
    public Dictionary<string, object> Data { get; set; } = new();

    public JsonElement? Expand { get; set; }
}
