using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels;
using ScrapWebsite.ViewModels.Public;
using codezone.ViewModels.Shared;

namespace ScrapWebsite.Services.Public;

public class PublicHomeQueryService : IPublicHomeQueryService
{
    private readonly AppDbContext _dbContext;
    private readonly IPublicSeoQueryService _seoQueryService;
    private readonly IPublicScrapQueryService _scrapQueryService;

    public PublicHomeQueryService(AppDbContext dbContext, IPublicSeoQueryService seoQueryService, IPublicScrapQueryService scrapQueryService)
    {
        _dbContext = dbContext;
        _seoQueryService = seoQueryService;
        _scrapQueryService = scrapQueryService;
    }

    public async Task<HomeViewModel> GetHomeAsync(CancellationToken cancellationToken)
    {
        var banner = await _dbContext.Banners
            .AsNoTracking()
            .Where(item => item.Status == PublicConstants.Active)
            .OrderBy(item => item.SortOrder)
            .Select(item => new BannerDto(
                item.Title,
                item.Subtitle,
                item.ImageUrl,
                item.PrimaryButtonText,
                item.PrimaryButtonUrl,
                item.SecondaryButtonText,
                item.SecondaryButtonUrl))
            .FirstOrDefaultAsync(cancellationToken);

        var scrapRows = await _dbContext.ScrapItems
            .AsNoTracking()
            .Where(item => item.Status == PublicConstants.Published && item.DeletedAt == null)
            .OrderByDescending(item => item.IsFeatured)
            .ThenBy(item => item.SortOrder)
            .ThenByDescending(item => item.PublishedAt)
            .ThenByDescending(item => item.Id)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.Slug,
                CategoryName = item.Category == null ? "Phế liệu" : item.Category.Name,
                item.ShortDescription,
                item.PrimaryImage,
                item.PriceFrom,
                item.PriceLabel,
                item.Unit,
                item.IsFeatured,
                item.SortOrder,
                item.PublishedAt
            })
            .Take(8)
            .ToListAsync(cancellationToken);

        var scrapItems = scrapRows
            .Select(item => new ScrapCardDto(
                item.Id,
                item.Name,
                item.Slug,
                item.CategoryName,
                item.ShortDescription,
                item.PrimaryImage,
                PriceTextBuilder.Build(item.PriceFrom, item.PriceLabel, item.Unit),
                item.Unit,
                item.IsFeatured,
                item.SortOrder,
                item.PublishedAt))
            .ToList();

        var latestPosts = await _dbContext.Posts
            .AsNoTracking()
            .Where(post => post.Status == PublicConstants.Published && post.DeletedAt == null)
            .OrderByDescending(post => post.PublishedAt)
            .ThenByDescending(post => post.Id)
            .Select(post => new PostCardDto(
                post.Id,
                post.Title,
                post.Slug,
                post.Category == null ? "Tin tức" : post.Category.Name,
                post.Excerpt,
                post.CoverImage,
                post.PublishedAt,
                post.IsFeatured,
                post.SortOrder))
            .Take(6)
            .ToListAsync(cancellationToken);

        var faqs = await _dbContext.FaqItems
            .AsNoTracking()
            .Where(faq => faq.DeletedAt == null && faq.Status == PublicConstants.Published && faq.EntityType == "home")
            .OrderBy(faq => faq.SortOrder)
            .ThenBy(faq => faq.Id)
            .Select(faq => new FaqItemViewModel(faq.Question, faq.Answer))
            .ToListAsync(cancellationToken);

        var categoryGroups = await _scrapQueryService.GetCategoryGroupsAsync(cancellationToken);

        var heroImageUrls = await _dbContext.SiteSettings
            .AsNoTracking()
            .Where(setting => setting.Key == "brand.banner_1" ||
                              setting.Key == "brand.banner_2" ||
                              setting.Key == "brand.banner_3")
            .OrderBy(setting => setting.Key)
            .Select(setting => setting.Value)
            .Where(value => value != null && value != "")
            .Select(value => value!)
            .ToListAsync(cancellationToken);

        var allHeroImages = heroImageUrls
            .Concat(new[] { banner?.ImageUrl }.Where(image => !string.IsNullOrWhiteSpace(image)).Select(image => image!))
            .Where(image => !string.IsNullOrWhiteSpace(image))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();

        var homeSettingKeys = new[]
        {
            "contact.phone",
            "contact.zalo",
            "contact.purchase_areas",
            "home.price_updated_text",
            "home.response_time_text",
            "home.about_image_main",
            "home.about_image_truck",
            "home.about_image_scale",
            "home.project_image_1",
            "home.project_image_2",
            "home.project_image_3",
            "home.referral_image",
            "home.final_cta_image"
        };

        var homeSettings = await _dbContext.SiteSettings
            .AsNoTracking()
            .Where(setting => homeSettingKeys.Contains(setting.Key))
            .Select(setting => new { setting.Key, setting.Value })
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value ?? string.Empty, cancellationToken);

        var hotline = GetSetting(homeSettings, "contact.phone", "0985565323");
        var zalo = GetSetting(homeSettings, "contact.zalo", hotline);
        var chrome = new HomeChromeDto(
            Hotline: hotline,
            HotlineHref: ToPhoneHref(hotline),
            ZaloHref: ToZaloHref(zalo),
            PriceUpdatedText: GetSetting(homeSettings, "home.price_updated_text", DateTime.Today.ToString("dd/MM/yyyy")),
            ResponseTimeText: GetSetting(homeSettings, "home.response_time_text", "30 phút"),
            PurchaseAreas: GetSetting(homeSettings, "contact.purchase_areas", "TP.HCM, Bình Dương, Đồng Nai")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            AboutImageMain: GetSetting(homeSettings, "home.about_image_main", "/assets/images/imported/brand/banner-1.jpg"),
            AboutImageTruck: GetSetting(homeSettings, "home.about_image_truck", "/assets/images/imported/brand/banner-2.jpg"),
            AboutImageScale: GetSetting(homeSettings, "home.about_image_scale", "/assets/images/imported/brand/banner-3.jpg"),
            ProjectImage1: GetSetting(homeSettings, "home.project_image_1", "/assets/images/imported/products/thumuasatvuncongtrinh8.jpg"),
            ProjectImage2: GetSetting(homeSettings, "home.project_image_2", "/assets/images/imported/products/thumuamaymoccuthanhly1.jpg"),
            ProjectImage3: GetSetting(homeSettings, "home.project_image_3", "/assets/images/imported/products/thumuadongcap1.jpg"),
            ReferralImage: GetSetting(homeSettings, "home.referral_image", "/assets/images/imported/brand/banner-2.jpg"),
            FinalCtaImage: GetSetting(homeSettings, "home.final_cta_image", "/assets/images/imported/brand/banner-3.jpg"));

        return new HomeViewModel
        {
            Seo = await _seoQueryService.GetByRouteAsync(
                "/",
                new SeoDto("Trang chủ", "Website thu mua phế liệu.", CanonicalUrl: "/"),
                cancellationToken),
            HeroBanner = banner ?? new BannerDto("Thu mua phế liệu giá cao", "Khảo sát nhanh, cân minh bạch, thanh toán ngay.", "/assets/images/imported/brand/banner-1.jpg", "Liên hệ báo giá", "/lien-he", null, null),
            HeroImageUrls = allHeroImages,
            Chrome = chrome,
            FeaturedScrapItems = scrapItems,
            CategoryGroups = categoryGroups,
            LatestPosts = latestPosts,
            Faq = new FaqAccordionViewModel("faqHome", faqs)
        };
    }

    private static string GetSetting(IReadOnlyDictionary<string, string> settings, string key, string fallback)
    {
        return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : fallback;
    }

    private static string ToPhoneHref(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? "#" : $"tel:{digits}";
    }

    private static string ToZaloHref(string zalo)
    {
        if (zalo.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            zalo.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return zalo;
        }

        var digits = new string(zalo.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? "#" : $"https://zalo.me/{digits}";
    }
}
