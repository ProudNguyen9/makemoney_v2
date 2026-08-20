using ScrapWebsite.Models;

namespace ScrapWebsite.ViewModels;

public class NewsIndexViewModel
{
    public SharedSeoViewModel Seo { get; set; } = new();

    public IReadOnlyList<Post> Posts { get; set; } = [];
}
