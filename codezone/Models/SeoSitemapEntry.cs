namespace ScrapWebsite.Models;

public class SeoSitemapEntry
{
    public int Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    public string RoutePath { get; set; } = string.Empty;

    public decimal Priority { get; set; }

    public string ChangeFrequency { get; set; } = "weekly";

    public bool IncludeInSitemap { get; set; } = true;

    public DateTime? LastModifiedAt { get; set; }
}
