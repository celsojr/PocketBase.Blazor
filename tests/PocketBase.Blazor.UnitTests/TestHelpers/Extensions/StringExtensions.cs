namespace PocketBase.Blazor.UnitTests.TestHelpers.Extensions;

using System.Text.RegularExpressions;

public static class StringExtensions
{
public static string ToSlug(this string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return string.Empty;

        string slug = text.ToLowerInvariant();
    
    // Replace invalid characters with hyphens
    slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
    
    // Replace multiple spaces with single space
    slug = Regex.Replace(slug, @"\s+", " ").Trim();
    
    // Replace spaces with hyphens
    slug = slug.Replace(" ", "-");
    
    return slug;
}
}
