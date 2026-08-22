using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.ViewModels;

public class NewsDetailViewModel
{
    public SeoDto Seo { get; set; } = new("Chi tiet bai viet", "Chi tiet tin tuc.");

    public PostDetailDto Post { get; set; } = new(0, string.Empty, string.Empty, "Tin tức", null, null, null, "Quản trị viên", DateTime.UtcNow);

    public IReadOnlyList<PostImageDto> Images { get; set; } = [];

    public IReadOnlyList<PostCardDto> RelatedPosts { get; set; } = [];
}
