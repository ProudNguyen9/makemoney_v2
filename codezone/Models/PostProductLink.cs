namespace ScrapWebsite.Models;

public class PostProductLink
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int ScrapItemId { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post? Post { get; set; }

    public ScrapItem? ScrapItem { get; set; }
}
