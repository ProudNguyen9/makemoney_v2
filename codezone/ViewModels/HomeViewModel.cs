using ScrapWebsite.ViewModels.Public;
using codezone.ViewModels.Shared;

namespace ScrapWebsite.ViewModels;

public class HomeViewModel
{
    public SeoDto Seo { get; set; } = new("ScrapWebsite", "Website thu mua phe lieu.");

    public BannerDto HeroBanner { get; set; } = new("Thu mua phe lieu gia cao", null, null, null, null, null, null);

    public IReadOnlyList<string> HeroImageUrls { get; set; } = [];

    public HomeChromeDto Chrome { get; set; } = new(
        "0985565323",
        "tel:0985565323",
        "https://zalo.me/0985565323",
        DateTime.Today.ToString("dd/MM/yyyy"),
        "30 phút",
        ["TP.HCM", "Bình Dương", "Đồng Nai"],
        "/assets/images/imported/brand/banner-1.jpg",
        "/assets/images/imported/brand/banner-2.jpg",
        "/assets/images/imported/brand/banner-3.jpg",
        "/assets/images/imported/products/thumuasatvuncongtrinh8.jpg",
        "/assets/images/imported/products/thumuamaymoccuthanhly1.jpg",
        "/assets/images/imported/products/thumuadongcap1.jpg",
        "/assets/images/imported/brand/banner-2.jpg",
        "/assets/images/imported/brand/banner-3.jpg");

    public IReadOnlyList<ScrapCardDto> FeaturedScrapItems { get; set; } = [];

    public IReadOnlyList<CategoryGroupCardDto> CategoryGroups { get; set; } = [];

    public IReadOnlyList<PostCardDto> LatestPosts { get; set; } = [];

    public FaqAccordionViewModel Faq { get; set; } = new("faqHome", []);
}
