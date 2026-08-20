using ScrapWebsite.Models;

namespace ScrapWebsite.ViewModels;

public class NewsDetailViewModel
{
    public SharedSeoViewModel Seo { get; set; } = new();

    public Post Post { get; set; } = new();

    public IReadOnlyList<Post> RelatedPosts { get; set; } = [];
}
