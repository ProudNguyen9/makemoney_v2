namespace ScrapWebsite.Models;

public class FaqItem
{
    public int Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    public string? RoutePath { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public string Status { get; set; } = "published";

    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
