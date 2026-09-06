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
        "media.hotline_overlay_color",
        "seo.site_title",
        "seo.default_description",
        "seo.default_og_title",
        "seo.default_og_image",
        "contact.whatsapp"
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

            var companyName = Get(settings, "site.name", "Phế Liệu Minh Đức");
            var hotline = Get(settings, "contact.phone", "0985565323");
            var zalo = Get(settings, "contact.zalo", hotline);
            var zaloHref = ToZaloHref(zalo);
            var whatsapp = Get(settings, "contact.whatsapp", hotline);
            var whatsappHref = ToWhatsAppHref(whatsapp);
            var address = Get(settings, "contact.address", "Hóc Môn, TP. Hồ Chí Minh");

            return new SiteChromeViewModel(
                CompanyName: companyName,
                Hotline: hotline,
                HotlineHref: ToPhoneHref(hotline),
                Email: Get(settings, "contact.email", "phelieuminhduc@gmail.com"),
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
                ResponseTimeText: Get(settings, "home.response_time_text", "30 phút"),
                HotlineOverlayColor: NormalizeHexColor(Get(settings, "media.hotline_overlay_color", string.Empty)) ?? string.Empty)
            {
                SiteTitle = Get(settings, "seo.site_title", string.Empty),
                DefaultDescription = Get(settings, "seo.default_description", "Thu mua phế liệu tận nơi giá cao, cân minh bạch, thanh toán nhanh."),
                DefaultOgTitle = Get(settings, "seo.default_og_title", string.Empty),
                DefaultOgImage = Get(settings, "seo.default_og_image", Get(settings, "site.default_og_image", "/assets/images/imported/brand/banner-1.jpg")),
                WhatsApp = whatsapp,
                WhatsAppHref = whatsappHref
            };
        })!;
    }

    private static string Get(IReadOnlyDictionary<string, string> settings, string key, string fallback)
    {
        return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : fallback;
    }

    /// <summary>Chỉ cho phép màu hex #RGB/#RRGGBB/#RRGGBBAA vì giá trị được nhúng thẳng vào CSS công khai.</summary>
    private static string? NormalizeHexColor(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }
        return System.Text.RegularExpressions.Regex.IsMatch(trimmed, "^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")
            ? trimmed.ToLowerInvariant()
            : null;
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

    private static string ToWhatsAppHref(string whatsapp)
    {
        if (string.IsNullOrWhiteSpace(whatsapp))
        {
            return "#";
        }

        if (whatsapp.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            whatsapp.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return whatsapp;
        }

        var digits = new string(whatsapp.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digits))
        {
            return "#";
        }

        if (digits.StartsWith("0"))
        {
            digits = "84" + digits[1..];
        }
        else if (!digits.StartsWith("84") && digits.Length <= 10)
        {
            digits = "84" + digits;
        }

        return $"https://wa.me/{digits}";
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
