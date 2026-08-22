using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace ScrapWebsite.Services.Public;

public sealed record PublicCursor(bool IsFeatured, int SortOrder, DateTime? PublishedAt, int Id);

public static class CursorToken
{
    public static string Encode(PublicCursor cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        return WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(json));
    }

    public static PublicCursor? Decode(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var json = System.Text.Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            return JsonSerializer.Deserialize<PublicCursor>(json);
        }
        catch
        {
            return null;
        }
    }
}
