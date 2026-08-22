namespace ScrapWebsite.Models;

public class PostCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public string Status { get; set; } = "published";

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
