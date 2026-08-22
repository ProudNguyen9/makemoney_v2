using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.ViewModels;

public class ScrapIndexViewModel
{
    public SeoDto Seo { get; set; } = new("Mat hang phe lieu", "Danh sach cac mat hang phe lieu thu mua.");

    public NumberedPageDto<ScrapCardDto> Page { get; set; } = new([], 1, 12, 0, 1);
}

public class ScrapCategoryPageViewModel
{
    public SeoDto Seo { get; set; } = new("Danh mục phế liệu", "Danh mục phế liệu thu mua theo nhóm.", CanonicalUrl: "/phe-lieu/danh-muc");

    public CategoryGroupCardDto? Current { get; set; }

    public string? CurrentDescription { get; set; }

    public IReadOnlyList<CategoryGroupCardDto> Groups { get; set; } = [];

    public IReadOnlyList<ScrapCardDto> Items { get; set; } = [];
}
