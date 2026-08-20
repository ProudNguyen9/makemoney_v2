namespace ScrapWebsite.Helpers;

public static class AssetHelper
{
    public static string NormalizePath(string? path, string fallback = "/images/shared/placeholder.svg")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return fallback;
        }

        var normalized = path.Replace('\\', '/').Trim();

        if (normalized.StartsWith("~/", StringComparison.Ordinal))
        {
            return $"/{normalized[2..]}";
        }

        return normalized.StartsWith('/') ? normalized : $"/{normalized}";
    }
}
