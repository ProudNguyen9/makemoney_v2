namespace ScrapWebsite.Models;

public class ScrapCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<ScrapItem> ScrapItems { get; set; } = new List<ScrapItem>();
}
