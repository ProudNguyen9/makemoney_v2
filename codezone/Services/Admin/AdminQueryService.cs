using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Areas.Admin.ViewModels.Data;
using ScrapWebsite.Data;

namespace ScrapWebsite.Services.Admin;

public sealed class AdminQueryService :
    IAdminDashboardQueryService,
    IAdminScrapQueryService,
    IAdminArticleQueryService,
    IAdminPriceQueryService,
    IAdminSeoQueryService,
    IAdminSettingsQueryService
{
    private const int AdminListLimit = 50;
    private readonly AppDbContext _dbContext;

    public AdminQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var scrapCount = await _dbContext.ScrapItems.AsNoTracking().CountAsync(cancellationToken);
        var postCount = await _dbContext.Posts.AsNoTracking().CountAsync(cancellationToken);
        var mediaCount = await _dbContext.MediaFiles.AsNoTracking().CountAsync(cancellationToken);
        var seoCount = await _dbContext.SeoMetadata.AsNoTracking().CountAsync(cancellationToken);
        var latestPriceDate = await _dbContext.ScrapPriceHistory.AsNoTracking()
            .OrderByDescending(price => price.EffectiveDate)
            .Select(price => (DateOnly?)price.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        var latestPosts = await QueryPostRows(_dbContext.Posts.AsNoTracking()
                .OrderByDescending(post => post.PublishedAt)
                .ThenByDescending(post => post.Id))
            .Take(4)
            .ToListAsync(cancellationToken);

        var featuredScrap = await QueryScrapRows(_dbContext.ScrapItems.AsNoTracking()
                .OrderByDescending(item => item.IsFeatured)
                .ThenBy(item => item.SortOrder)
                .ThenByDescending(item => item.PublishedAt))
            .Take(6)
            .ToListAsync(cancellationToken);

        return new AdminDashboardViewModel(
            scrapCount,
            postCount,
            mediaCount,
            seoCount,
            LeadCount: 0,
            latestPriceDate,
            latestPosts,
            featuredScrap);
    }

    public async Task<AdminScrapListViewModel> GetScrapListAsync(string? group, string? status, string? query, CancellationToken cancellationToken)
    {
        var categories = await _dbContext.ScrapCategories.AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new AdminCategoryOptionDto(category.Id, category.Name, category.Slug))
            .ToListAsync(cancellationToken);

        var baseQuery = _dbContext.ScrapItems.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(group))
        {
            baseQuery = baseQuery.Where(item => item.Category != null && item.Category.Slug == group);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery = baseQuery.Where(item => item.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(item => item.Name.Contains(query) || item.Slug.Contains(query));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await QueryScrapRows(baseQuery
                .OrderByDescending(item => item.IsFeatured)
                .ThenBy(item => item.SortOrder)
                .ThenBy(item => item.Name))
            .Take(AdminListLimit)
            .ToListAsync(cancellationToken);

        return new AdminScrapListViewModel(categories, items, group, status, query, totalCount);
    }

    public async Task<AdminArticleListViewModel> GetArticleListAsync(string? category, string? status, string? query, CancellationToken cancellationToken)
    {
        var categories = await _dbContext.PostCategories.AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(item => new AdminCategoryOptionDto(item.Id, item.Name, item.Slug))
            .ToListAsync(cancellationToken);

        var baseQuery = _dbContext.Posts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(category))
        {
            baseQuery = baseQuery.Where(post => post.Category != null && post.Category.Slug == category);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery = baseQuery.Where(post => post.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(post => post.Title.Contains(query) || post.Slug.Contains(query));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await QueryPostRows(baseQuery
                .OrderByDescending(post => post.PublishedAt)
                .ThenByDescending(post => post.Id))
            .Take(AdminListLimit)
            .ToListAsync(cancellationToken);

        return new AdminArticleListViewModel(categories, items, category, status, query, totalCount);
    }

    public async Task<AdminPriceListViewModel> GetPriceListAsync(string? group, string? query, CancellationToken cancellationToken)
    {
        var categories = await _dbContext.ScrapCategories.AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new AdminCategoryOptionDto(category.Id, category.Name, category.Slug))
            .ToListAsync(cancellationToken);

        var baseQuery = _dbContext.ScrapPrices.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(group))
        {
            baseQuery = baseQuery.Where(price => price.ScrapItem != null && price.ScrapItem.Category != null && price.ScrapItem.Category.Slug == group);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(price => price.ScrapItem != null && price.ScrapItem.Name.Contains(query));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(price => price.ScrapItem != null && price.ScrapItem.Category != null ? price.ScrapItem.Category.SortOrder : 999)
            .ThenBy(price => price.ScrapItem != null ? price.ScrapItem.SortOrder : 999)
            .ThenByDescending(price => price.EffectiveDate)
            .Select(price => new AdminPriceRowDto(
                price.Id,
                price.ScrapItem != null ? price.ScrapItem.Name : "Chưa gắn loại",
                price.ScrapItem != null && price.ScrapItem.Category != null ? price.ScrapItem.Category.Name : "Chưa phân nhóm",
                price.PriceValue,
                BuildPriceText(price.PriceLabel, price.PriceValue, price.Unit),
                price.Unit ?? "kg",
                price.EffectiveDate,
                "active"))
            .Take(AdminListLimit)
            .ToListAsync(cancellationToken);

        var lastUpdatedAt = await _dbContext.ScrapPriceHistory.AsNoTracking()
            .OrderByDescending(price => price.EffectiveDate)
            .Select(price => (DateOnly?)price.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        return new AdminPriceListViewModel(categories, items, group, query, totalCount, lastUpdatedAt);
    }

    public async Task<AdminSeoListViewModel> GetSeoListAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.SeoMetadata.AsNoTracking()
            .OrderBy(seo => seo.RoutePath == null)
            .ThenBy(seo => seo.RoutePath)
            .ThenBy(seo => seo.EntityType)
            .ThenBy(seo => seo.EntityId)
            .Select(seo => new AdminSeoRowDto(
                seo.Id,
                seo.EntityType,
                seo.EntityId,
                seo.RoutePath ?? "",
                seo.SeoTitle ?? "",
                seo.MetaDescription ?? "",
                seo.OgImage ?? "",
                seo.RobotsIndex,
                seo.RobotsFollow,
                seo.Status))
            .Take(AdminListLimit)
            .ToListAsync(cancellationToken);

        var settings = await LoadSettingsAsync(cancellationToken);
        var sitemapCount = await _dbContext.SeoSitemapEntries.AsNoTracking().CountAsync(cancellationToken);
        var redirectCount = await _dbContext.SeoRedirects.AsNoTracking().CountAsync(cancellationToken);

        return new AdminSeoListViewModel(
            rows,
            sitemapCount,
            redirectCount,
            Get(settings, "seo.site_title", Get(settings, "site.name", "Phế Liệu Thành Trung")),
            Get(settings, "seo.default_description", "Thu mua phế liệu tận nơi giá cao, cân minh bạch, thanh toán nhanh."),
            Get(settings, "seo.default_og_image", Get(settings, "site.default_og_image", "/assets/images/imported/brand/banner-1.jpg")));
    }

    public async Task<AdminSettingsViewModel> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await LoadSettingsAsync(cancellationToken);

        return new AdminSettingsViewModel(
            Get(settings, "site.name", "Phế Liệu Thành Trung"),
            Get(settings, "company.tax_code", "Đang cập nhật"),
            Get(settings, "contact.address", Get(settings, "contact.warehouse_address", "TP.HCM")),
            Get(settings, "contact.phone", "0974640626"),
            Get(settings, "contact.zalo", "0974640626"),
            Get(settings, "contact.email", "phelieuthanhtrung@gmail.com"),
            Get(settings, "contact.working_hours", "T2-CN: 7:00 - 20:00"),
            Get(settings, "social.facebook", ""),
            Get(settings, "social.youtube", ""),
            Get(settings, "social.tiktok", ""),
            Get(settings, "site.logo", "/assets/images/imported/brand/logo.png"),
            Get(settings, "site.footer_logo", "/assets/images/imported/brand/logo-footer.png"),
            Get(settings, "site.favicon", "/favicon.ico"),
            Get(settings, "system.cache_minutes", "5"));
    }

    private static IQueryable<AdminScrapRowDto> QueryScrapRows(IQueryable<Models.ScrapItem> query)
    {
        return query.Select(item => new AdminScrapRowDto(
            item.Id,
            item.Name,
            item.Slug,
            item.Category != null ? item.Category.Name : "Chưa phân nhóm",
            item.PrimaryImage,
            BuildPriceText(item.PriceLabel, item.PriceFrom, item.Unit),
            item.Status,
            item.IsFeatured,
            item.SortOrder,
            item.PublishedAt));
    }

    private static IQueryable<AdminPostRowDto> QueryPostRows(IQueryable<Models.Post> query)
    {
        return query.Select(post => new AdminPostRowDto(
            post.Id,
            post.Title,
            post.Slug,
            post.Category != null ? post.Category.Name : "Chưa phân loại",
            post.CoverImage,
            post.Status,
            post.PublishedAt,
            post.AuthorName));
    }

    private async Task<Dictionary<string, string>> LoadSettingsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SiteSettings.AsNoTracking()
            .Select(setting => new { setting.Key, setting.Value })
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value ?? "", cancellationToken);
    }

    private static string Get(IReadOnlyDictionary<string, string> settings, string key, string fallback)
    {
        return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static string BuildPriceText(string? priceLabel, decimal? value, string? unit)
    {
        if (!string.IsNullOrWhiteSpace(priceLabel))
        {
            return priceLabel;
        }

        return value.HasValue
            ? $"{value.Value:N0} đ/{(string.IsNullOrWhiteSpace(unit) ? "kg" : unit)}"
            : "Liên hệ";
    }
}
