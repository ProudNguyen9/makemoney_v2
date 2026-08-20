using ScrapWebsite.Models;

namespace ScrapWebsite.ViewModels;

public class ScrapDetailViewModel
{
    public SharedSeoViewModel Seo { get; set; } = new();

    public ScrapItem Item { get; set; } = new();

    public IReadOnlyList<ScrapPrice> Prices { get; set; } = [];

    public IReadOnlyList<ScrapItem> RelatedItems { get; set; } = [];
}
