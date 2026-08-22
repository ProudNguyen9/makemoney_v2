namespace ScrapWebsite.Models;

public class PostImage
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public int OrderIndex { get; set; }

    public Post? Post { get; set; }
}
