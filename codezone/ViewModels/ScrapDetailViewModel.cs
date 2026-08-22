using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.ViewModels;

public class ScrapDetailViewModel
{
    public SeoDto Seo { get; set; } = new("Chi tiet phe lieu", "Chi tiet mat hang phe lieu.");

    public ScrapDetailDto Item { get; set; } = new(0, string.Empty, string.Empty, "Phế liệu", null, null, null, "Liên hệ báo giá", "kg", []);

    public IReadOnlyList<ScrapCardDto> RelatedItems { get; set; } = [];
}
