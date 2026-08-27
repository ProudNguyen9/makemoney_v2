using codezone.ViewModels.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ScrapWebsite.Data;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Services.Public;

public sealed class SiteChromeService : ISiteChromeService
{
    private const string CacheKey = "public:site-chrome";

    private static readonly string[] Keys =
    [
        "site.name",
        "contact.phone",
        "contact.email",
        "contact.zalo",
        "contact.address",
        "contact.warehouse_address",
        "contact.working_hours",
        "company.tax_code",
        "site.logo",
        "site.footer_logo",
        "site.favicon",
        "brand.logo",
        "brand.logo_footer",
        "social.facebook",
        "contact.purchase_areas",
        "brand.default_hero_image",
        "brand.default_cta_image",
        "home.response_time_text",
        "seo.site_title"
    ];

    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public SiteChromeService(AppDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public Task<SiteChromeViewModel> GetAsync(CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(1);

            var settings = await _dbContext.SiteSettings
                .AsNoTracking()
                .Where(setting => Keys.Contains(setting.Key))
                .Select(setting => new { setting.Key, setting.Value })
                .ToDictionaryAsync(setting => setting.Key, setting => setting.Value ?? string.Empty, cancellationToken);

            var companyName = Get(settings, "site.name", "Thành Trung");
            var hotline = Get(settings, "contact.phone", "0974640626");
            var zalo = Get(settings, "contact.zalo", hotline);
            var zaloHref = ToZaloHref(zalo);
            var address = Get(settings, "contact.address", "Hóc Môn, TP. Hồ Chí Minh");

            return new SiteChromeViewModel(
                CompanyName: companyName,
                Hotline: hotline,
                HotlineHref: ToPhoneHref(hotline),
                Email: Get(settings, "contact.email", "phelieuthanhtrung@gmail.com"),
                Zalo: zalo,
                ZaloHref: zaloHref,
                MessengerHref: ToMessengerHref(Get(settings, "social.facebook", string.Empty)),
                Address: address,
                WarehouseAddress: Get(settings, "contact.warehouse_address", address),
                WorkingHours: Get(settings, "contact.working_hours", "7:00 - 20:00"),
                TaxCode: Get(settings, "company.tax_code", "Đang cập nhật"),
                LogoUrl: Get(settings, "site.logo", Get(settings, "brand.logo", "/assets/images/imported/brand/logo.png")),
                FooterLogoUrl: Get(settings, "site.footer_logo", Get(settings, "brand.logo_footer", "/assets/images/imported/brand/logo-footer.png")),
                FaviconUrl: Get(settings, "site.favicon", "/assets/images/imported/brand/favicon.png"),
                PurchaseAreas: Get(settings, "contact.purchase_areas", "TP.HCM, Bình Dương, Đồng Nai"),
                DefaultHeroImage: Get(settings, "brand.default_hero_image", "/assets/images/imported/brand/banner-1.jpg"),
                DefaultCtaImage: Get(settings, "brand.default_cta_image", "/assets/images/imported/brand/banner-3.jpg"),
                ResponseTimeText: Get(settings, "home.response_time_text", "30 phút"))
            {
                SiteTitle = Get(settings, "seo.site_title", string.Empty)
            };
        })!;
    }

    private static string Get(IReadOnlyDictionary<string, string> settings, string key, string fallback)
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

    private static string ToMessengerHref(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "#";
        }

        return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? value
            : $"https://m.me/{value.Trim().TrimStart('@')}";
    }
}
