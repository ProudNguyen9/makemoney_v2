namespace ScrapWebsite.Models;

public class SiteSetting
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string Group { get; set; } = "general";
}
