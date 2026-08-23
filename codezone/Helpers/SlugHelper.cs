using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ScrapWebsite.Helpers;

public static partial class SlugHelper
{
    public static string ToSlug(string value)
    {
        // U+0111 (đ/Đ) does not decompose under NFD, so map it to a plain d first.
        var normalized = value.Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = NonWordRegex().Replace(slug, "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "item" : slug;
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonWordRegex();
}
