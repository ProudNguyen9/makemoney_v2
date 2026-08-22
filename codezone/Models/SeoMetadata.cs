namespace ScrapWebsite.Models;

public class SeoMetadata
{
    public int Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    public string? RoutePath { get; set; }

    public string SeoTitle { get; set; } = string.Empty;

    public string? MetaDescription { get; set; }

    public string? Keywords { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? OgTitle { get; set; }

    public string? OgDescription { get; set; }

    public string? OgImage { get; set; }

    public string OgType { get; set; } = "website";

    public bool RobotsIndex { get; set; } = true;

    public bool RobotsFollow { get; set; } = true;

    public string Status { get; set; } = "active";
}
