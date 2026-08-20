using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Helpers;

public static class SeoHelper
{
    public static SharedSeoViewModel Build(string title, string? description = null, string? keywords = null)
    {
        return new SharedSeoViewModel
        {
            Title = title,
            Description = description ?? "Website thu mua phe lieu.",
            Keywords = keywords
        };
    }
}
