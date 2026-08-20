namespace ScrapWebsite.Helpers;

public static class TextHelper
{
    public static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : $"{trimmed[..Math.Max(0, maxLength - 3)]}...";
    }
}
