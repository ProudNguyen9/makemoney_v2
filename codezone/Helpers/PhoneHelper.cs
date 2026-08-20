namespace ScrapWebsite.Helpers;

public static class PhoneHelper
{
    public static string ToTelHref(string? phone)
    {
        var digits = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? "#" : $"tel:{digits}";
    }
}
