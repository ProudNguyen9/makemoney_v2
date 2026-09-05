namespace ScrapWebsite.Models;

public class Service
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    public string? ContentHtml { get; set; }

    public string? CoverImage { get; set; }

    public string? IconCss { get; set; }

    public string? SeoKeywords { get; set; }

    public string Status { get; set; } = "published";

    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
