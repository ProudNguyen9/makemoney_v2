namespace ScrapWebsite.Areas.Admin.ViewModels.Data;

public sealed record AdminDashboardViewModel(
    int ScrapItemCount,
    int PostCount,
    int MediaFileCount,
    int SeoMetadataCount,
    int LeadCount,
    DateOnly? LatestPriceDate,
    IReadOnlyList<AdminPostRowDto> LatestPosts,
    IReadOnlyList<AdminScrapRowDto> FeaturedScrapItems);

public sealed record AdminScrapListViewModel(
    IReadOnlyList<AdminCategoryOptionDto> Categories,
    IReadOnlyList<AdminScrapRowDto> Items,
    string? Group,
    string? Status,
    string? Query,
    int TotalCount);

public sealed record AdminArticleListViewModel(
    IReadOnlyList<AdminCategoryOptionDto> Categories,
    IReadOnlyList<AdminPostRowDto> Items,
    string? Category,
    string? Status,
    string? Query,
    int TotalCount);

public sealed record AdminPriceListViewModel(
    IReadOnlyList<AdminCategoryOptionDto> Categories,
    IReadOnlyList<AdminPriceRowDto> Items,
    string? Group,
    string? Query,
    int TotalCount,
    DateOnly? LastUpdatedAt);

public sealed record AdminSeoListViewModel(
    IReadOnlyList<AdminSeoRowDto> Items,
    int SitemapCount,
    int RedirectCount,
    string SiteTitle,
    string DefaultDescription,
    string DefaultOgImage);

public sealed record AdminSettingsViewModel(
    string CompanyName,
    string TaxCode,
    string Address,
    string Hotline,
    string Zalo,
    string Email,
    string WorkingHours,
    string Facebook,
    string Youtube,
    string Tiktok,
    string LogoUrl,
    string FooterLogoUrl,
    string FaviconUrl,
    string CacheMinutes);

public sealed record AdminScrapRowDto(
    int Id,
    string Name,
    string Slug,
    string CategoryName,
    string? ImageUrl,
    string PriceText,
    string Status,
    bool IsFeatured,
    int SortOrder,
    DateTime? PublishedAt);

public sealed record AdminPostRowDto(
    int Id,
    string Title,
    string Slug,
    string CategoryName,
    string? CoverImage,
    string Status,
    DateTime? PublishedAt,
    string? AuthorName);

public sealed record AdminPriceRowDto(
    int Id,
    string ScrapName,
    string CategoryName,
    decimal? PriceValue,
    string PriceText,
    string Unit,
    DateOnly? EffectiveDate,
    string Status);

public sealed record AdminSeoRowDto(
    int Id,
    string EntityType,
    int? EntityId,
    string RoutePath,
    string SeoTitle,
    string MetaDescription,
    string OgImage,
    bool RobotsIndex,
    bool RobotsFollow,
    string Status);

public sealed record AdminCategoryOptionDto(int Id, string Name, string Slug);
