namespace ScrapWebsite.Models;

public class ScrapImage
{
    public int Id { get; set; }

    public int ScrapItemId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public int OrderIndex { get; set; }

    public ScrapItem? ScrapItem { get; set; }
}
