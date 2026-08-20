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

    public bool IsFeatured { get; set; }

    public ScrapCategory? Category { get; set; }

    public ICollection<ScrapImage> Images { get; set; } = new List<ScrapImage>();

    public ICollection<ScrapPrice> Prices { get; set; } = new List<ScrapPrice>();
}
