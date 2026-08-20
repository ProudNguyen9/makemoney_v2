namespace ScrapWebsite.Models;

public class Post
{
    public int Id { get; set; }

    public int? PostCategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    public string? Content { get; set; }

    public string? CoverImage { get; set; }

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public PostCategory? Category { get; set; }
}
