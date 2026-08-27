namespace ScrapWebsite.Models;

public class ScrapItem
{
    public int Id { get; set; }

    public int? ScrapCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? PrimaryImage { get; set; }

    public string? Unit { get; set; } = "kg";

    public decimal? PriceFrom { get; set; }

    public string? PriceLabel { get; set; }

    public string Status { get; set; } = "published";

    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; }

    public ScrapCategory? Category { get; set; }

    public ICollection<ScrapImage> Images { get; set; } = new List<ScrapImage>();

    public ICollection<ScrapPrice> Prices { get; set; } = new List<ScrapPrice>();
}
