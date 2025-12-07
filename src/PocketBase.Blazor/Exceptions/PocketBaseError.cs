using System;

namespace PocketBase.Blazor.Exceptions;

public class PocketBaseError
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public object? Data { get; set; }

    public bool RequiresApiKey =>
        Status == 403 || Message.Contains("superusers", StringComparison.OrdinalIgnoreCase);
}
