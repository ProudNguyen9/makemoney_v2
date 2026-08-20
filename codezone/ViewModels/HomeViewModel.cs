using ScrapWebsite.Models;

namespace ScrapWebsite.ViewModels;

public class HomeViewModel
{
    public SharedSeoViewModel Seo { get; set; } = new();

    public Banner HeroBanner { get; set; } = new();

    public IReadOnlyList<ScrapItem> FeaturedScrapItems { get; set; } = [];

    public IReadOnlyList<Post> LatestPosts { get; set; } = [];
}
