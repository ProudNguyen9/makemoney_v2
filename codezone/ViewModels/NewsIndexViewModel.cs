using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.ViewModels;

public class NewsIndexViewModel
{
    public SeoDto Seo { get; set; } = new("Tin tuc", "Tin tuc va kinh nghiem thu mua phe lieu.");

    public NewsChromeDto Chrome { get; set; } = new(
        "/assets/images/imported/brand/seo-og-image.png",
        "Tin tức & kiến thức phế liệu",
        "Cập nhật giá phế liệu, kinh nghiệm thanh lý và thông tin thu mua mới nhất.");

    public int PageSize { get; set; } = 10;

    public CursorPageDto<PostCardDto> Page { get; set; } = new([], null, false);

    public IReadOnlyList<PostCardDto> SidebarPosts { get; set; } = [];
}
