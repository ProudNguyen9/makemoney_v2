namespace ScrapWebsite.Models;

public class Banner
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public string? ImageUrl { get; set; }

    public string? PrimaryButtonText { get; set; }

    public string? PrimaryButtonUrl { get; set; }

    public string? SecondaryButtonText { get; set; }

    public string? SecondaryButtonUrl { get; set; }

    public int SortOrder { get; set; }

    public string Status { get; set; } = "active";
}
