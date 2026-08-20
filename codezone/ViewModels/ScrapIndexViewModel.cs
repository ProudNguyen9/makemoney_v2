using ScrapWebsite.Models;

namespace ScrapWebsite.ViewModels;

public class ScrapIndexViewModel
{
    public SharedSeoViewModel Seo { get; set; } = new();

    public IReadOnlyList<ScrapItem> Items { get; set; } = [];
}
