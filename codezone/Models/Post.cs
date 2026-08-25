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

    public string Status { get; set; } = "published";

    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }

    public string? AuthorName { get; set; }

    public string? SeoKeywords { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; }

    public PostCategory? Category { get; set; }

    public ICollection<PostImage> Images { get; set; } = new List<PostImage>();

    public ICollection<PostProductLink> ProductLinks { get; set; } = new List<PostProductLink>();
}
