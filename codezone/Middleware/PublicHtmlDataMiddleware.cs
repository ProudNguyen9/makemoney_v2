using System.Text;
using System.Text.RegularExpressions;
using codezone.ViewModels.Shared;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Middleware;

public sealed class PublicHtmlDataMiddleware
{
    private readonly RequestDelegate _next;

    public PublicHtmlDataMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISiteChromeService chromeService, IPublicPageContentService pageContentService)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        context.Response.Body = originalBody;

        if (!IsHtml(context.Response.ContentType))
        {
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody, context.RequestAborted);
            return;
        }

        buffer.Position = 0;
        using var reader = new StreamReader(buffer, Encoding.UTF8);
        var html = await reader.ReadToEndAsync(context.RequestAborted);
        var chrome = await chromeService.GetAsync(context.RequestAborted);
        var settings = await pageContentService.GetSettingsAsync(context.RequestAborted);
        html = ApplyPublicData(html, chrome, settings);

        var output = Encoding.UTF8.GetBytes(html);
        context.Response.ContentLength = output.Length;
        await originalBody.WriteAsync(output, context.RequestAborted);
    }

    private static bool ShouldSkip(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/assets", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHtml(string? contentType)
    {
        return contentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string ApplyPublicData(string html, SiteChromeViewModel chrome, IReadOnlyDictionary<string, string> settings)
    {
        var responseTime = Get(settings, "home.response_time_text", chrome.ResponseTimeText);
        var updatedText = Get(settings, "home.price_updated_text", DateTime.Today.ToString("dd/MM/yyyy"));
        var heroImage = Get(settings, "brand.default_hero_image", chrome.DefaultHeroImage);
        var ctaImage = Get(settings, "brand.default_cta_image", chrome.DefaultCtaImage);
        var mapImage = Get(settings, "public.contact.map_image", "/assets/images/imported/brand/banner-1.jpg");
        var truckImage = Get(settings, "public.image.truck", "/assets/images/imported/brand/banner-2.jpg");
        var yardImage = Get(settings, "public.image.yard", "/assets/images/imported/brand/banner-1.jpg");
        var teamImage = Get(settings, "public.image.team", "/assets/images/imported/brand/banner-3.jpg");
        var scaleImage = Get(settings, "public.image.scale", "/assets/images/imported/brand/banner-3.jpg");
        var scrapImage = Get(settings, "public.image.scrap", "/assets/images/imported/products/thumuasatvuncongtrinh8.jpg");
        var projectImage = Get(settings, "public.image.project", "/assets/images/imported/products/thumuamaymoccuthanhly1.jpg");
        var newsImage = Get(settings, "public.image.news", "/assets/images/imported/brand/seo-og-image.png");

        var replacements = new Dictionary<string, string>
        {
            ["[HOTLINE]"] = chrome.Hotline,
            ["[ZALO]"] = chrome.Zalo,
            ["[EMAIL]"] = chrome.Email,
            ["[ĐỊA CHỈ]"] = chrome.WarehouseAddress,
            ["[GIỜ LÀM VIỆC]"] = chrome.WorkingHours,
            ["[TÊN CÔNG TY]"] = chrome.CompanyName,
            ["[DD/MM/YYYY]"] = updatedText,
            ["[30 phút]"] = responseTime,
            ["tel:[HOTLINE]"] = chrome.HotlineHref,
            ["https://zalo.me/[ZALO]"] = chrome.ZaloHref,
            ["mailto:[EMAIL]"] = $"mailto:{chrome.Email}",
            ["~/assets/images/hero/hero-01.svg"] = heroImage,
            ["~/assets/images/hero/hero-02.svg"] = ctaImage,
            ["/assets/images/hero/hero-01.svg"] = heroImage,
            ["/assets/images/hero/hero-02.svg"] = ctaImage,
            ["~/assets/images/locations/location-map.svg"] = mapImage,
            ["/assets/images/locations/location-map.svg"] = mapImage,
            ["/assets/images/locations/location-dongnai.svg"] = mapImage,
            ["/assets/images/company/company-truck.svg"] = truckImage,
            ["/assets/images/company/company-yard.svg"] = yardImage,
            ["/assets/images/company/company-team.svg"] = teamImage,
            ["/assets/images/company/company-warehouse.svg"] = yardImage,
            ["/assets/images/company/company-scale.svg"] = scaleImage,
            ["/assets/images/scrap/scrap-misc.svg"] = scrapImage,
            ["/assets/images/scrap/scrap-copper.svg"] = scrapImage,
            ["/assets/images/scrap/scrap-cable.svg"] = scrapImage,
            ["/assets/images/scrap/scrap-iron.svg"] = scrapImage,
            ["/assets/images/scrap/scrap-motor.svg"] = scrapImage,
            ["/assets/images/projects/project-01-cover.svg"] = projectImage,
            ["/assets/images/projects/project-02-cover.svg"] = projectImage,
            ["/assets/images/projects/project-03-cover.svg"] = projectImage,
            ["/assets/images/projects/project-04-cover.svg"] = projectImage,
            ["/assets/images/projects/project-05-cover.svg"] = projectImage,
            ["/assets/images/projects/project-06-cover.svg"] = projectImage,
            ["/assets/images/news/news-01.svg"] = newsImage,
            ["/assets/images/news/news-02.svg"] = newsImage,
            ["/assets/images/news/news-03.svg"] = newsImage,
            ["/assets/images/news/news-04.svg"] = newsImage,
            ["/assets/images/news/news-05.svg"] = newsImage
        };

        foreach (var replacement in replacements)
        {
            html = html.Replace(replacement.Key, replacement.Value, StringComparison.OrdinalIgnoreCase);
        }

        return ReplaceRemainingBracketPlaceholders(html, settings);
    }

    private static string ReplaceRemainingBracketPlaceholders(string html, IReadOnlyDictionary<string, string> settings)
    {
        var genericMetric = Get(settings, "public.metric.generic", "20");
        var searchCount = Get(settings, "public.search.result_count", "12");

        return Regex.Replace(html, @"\[(XX|X|20XX|nếu có)\]", match =>
        {
            var value = match.Groups[1].Value.Trim();
            return value switch
            {
                "XX" => genericMetric,
                "X" => searchCount,
                "20XX" => "2014",
                "nếu có" => "theo điều khoản hợp đồng",
                _ => match.Value
            };
        });
    }

    private static string Get(IReadOnlyDictionary<string, string> settings, string key, string fallback)
    {
        return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : fallback;
    }
}
