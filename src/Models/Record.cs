using System;

namespace PocketBase.Blazor.Models;

public class Record
{
    public string Id { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
