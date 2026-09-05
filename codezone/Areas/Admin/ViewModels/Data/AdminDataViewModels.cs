namespace ScrapWebsite.Areas.Admin.ViewModels.Data;

public sealed record AdminPaginationViewModel(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record AdminDashboardViewModel(
    int ScrapItemCount,
    int PostCount,
    int MediaFileCount,
    int SeoMetadataCount,
    int LocationCount,
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
    int Page,
    int TotalCount)
{
    public AdminPaginationViewModel Pager => new(Page, 20, TotalCount, (int)Math.Ceiling(TotalCount / 20.0));
}

public sealed record AdminScrapCategoryListViewModel(
    IReadOnlyList<AdminScrapCategoryRowDto> Items);

public sealed record AdminScrapCategoryRowDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string Status,
    int SortOrder,
    int ItemCount);

public sealed record AdminArticleListViewModel(
    IReadOnlyList<AdminCategoryOptionDto> Categories,
    IReadOnlyList<AdminPostRowDto> Items,
    string? Category,
    string? Status,
    string? Query,
    int TotalCount)
{
    public bool IsTrashView => Status == "deleted";
    public bool IsFeaturedView => Status == "featured";
}

public sealed record AdminPriceListViewModel(
    IReadOnlyList<AdminCategoryOptionDto> Categories,
    IReadOnlyList<AdminPriceRowDto> Items,
    string? Group,
    string? Status,
    string? Query,
    int Page,
    int TotalCount,
    DateOnly? LastUpdatedAt)
{
    public AdminPaginationViewModel Pager => new(Page, 20, TotalCount, (int)Math.Ceiling(TotalCount / 20.0));
}

public sealed record AdminLeadListViewModel(
    IReadOnlyList<AdminLeadRowDto> Items,
    IReadOnlyList<string> ScrapTypes,
    IReadOnlyList<string> Areas,
    string? Status,
    string? Scrap,
    string? Area,
    string? Query,
    int Page,
    int TotalCount)
{
    public AdminPaginationViewModel Pager => new(Page, 20, TotalCount, (int)Math.Ceiling(TotalCount / 20.0));
}

public sealed record AdminServiceListViewModel(
    IReadOnlyList<AdminServiceRowDto> Items,
    string? Status,
    string? Query,
    int Page,
    int TotalCount)
{
    public AdminPaginationViewModel Pager => new(Page, 20, TotalCount, (int)Math.Ceiling(TotalCount / 20.0));
}

public sealed record AdminLocationListViewModel(
    IReadOnlyList<string> Provinces,
    IReadOnlyList<AdminLocationRowDto> Items,
    string? Province,
    string? Status,
    string? Query,
    int Page,
    int TotalCount)
{
    public AdminPaginationViewModel Pager => new(Page, 20, TotalCount, (int)Math.Ceiling(TotalCount / 20.0));
}

public sealed record AdminProjectListViewModel(
    IReadOnlyList<string> ProjectTypes,
    IReadOnlyList<AdminProjectRowDto> Items,
    string? ProjectType,
    string? Status,
    string? Query,
    int Page,
    int TotalCount)
{
    public AdminPaginationViewModel Pager => new(Page, 20, TotalCount, (int)Math.Ceiling(TotalCount / 20.0));
}

public sealed record AdminSeoListViewModel(
    IReadOnlyList<AdminSeoRowDto> Items,
    IReadOnlyList<string> EntityTypes,
    int SitemapCount,
    int RedirectCount,
    string SiteTitle,
    string DefaultDescription,
    string DefaultOgTitle,
    string DefaultOgImage,
    string? EntityType,
    string? Status,
    string? IndexState,
    string? Query);

public sealed record AdminSettingsViewModel(
    string CompanyName,
    string TaxCode,
    string Address,
    string Hotline,
    string Zalo,
    string Email,
    string WorkingHours,
    string PurchaseAreas,
    string Facebook,
    string LogoUrl,
    string FooterLogoUrl,
    string FaviconUrl,
    string PriceUpdatedText,
    string ResponseTimeText,
    string CacheMinutes,
    string SmtpHost,
    int SmtpPort,
    bool SmtpEnableSsl,
    string SmtpUserName,
    bool SmtpHasPassword,
    string SmtpFromEmail,
    string SmtpFromName,
    string SmtpToEmail,
    string HotlineOverlayColor);

public sealed record AdminMediaListViewModel(
    IReadOnlyList<AdminMediaGroupOptionDto> GroupOptions,
    IReadOnlyList<AdminMediaGroupDto> Groups,
    string? Group,
    string? Query);

public sealed record AdminMediaGroupOptionDto(
    string Key,
    string Name,
    int Count);

public sealed record AdminMediaGroupDto(
    string Key,
    string Name,
    IReadOnlyList<AdminMediaItemDto> Items);

public sealed record AdminMediaItemDto(
    string Key,
    string GroupKey,
    string GroupName,
    string Label,
    string Description,
    string RecommendedSize,
    string Url);

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
    string? AuthorName,
    bool IsFeatured,
    DateTime? DeletedAt);

public sealed record AdminLinkedProductDto(
    int Id,
    string Name,
    string Slug,
    string CategoryName,
    string Status,
    string? ImageUrl,
    string? PriceText,
    string? ShortDescription);

public sealed record AdminLeadRowDto(
    int Id,
    string Code,
    string CustomerName,
    string Phone,
    string? Zalo,
    string? ScrapType,
    string? QuantityText,
    string? Area,
    string? Message,
    string SourceForm,
    string? SourceUrl,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<string> ImageUrls);

public sealed record AdminLeadDetailDto(
    int Id,
    string Code,
    string CustomerName,
    string Phone,
    string? Zalo,
    string? Email,
    string? ScrapType,
    string? QuantityText,
    string? Area,
    string? Message,
    string SourceForm,
    string? SourceUrl,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<string> ImageUrls);

public sealed record AdminPriceRowDto(
    int Id,
    int ScrapItemId,
    string ScrapName,
    string CategoryName,
    decimal? PriceValue,
    string PriceText,
    string Unit,
    DateOnly? EffectiveDate,
    string Status,
    bool ItemIsPublished);

public sealed record AdminSeoRowDto(
    int Id,
    string EntityType,
    int? EntityId,
    string RoutePath,
    string SeoTitle,
    string MetaDescription,
    string OgTitle,
    string OgDescription,
    string OgImage,
    bool RobotsIndex,
    bool RobotsFollow,
    string Status);

public sealed record AdminCategoryOptionDto(int Id, string Name, string Slug);

public sealed record AdminServiceRowDto(
    int Id,
    string Title,
    string Slug,
    string? CoverImage,
    string? IconCss,
    string Status,
    bool IsFeatured,
    int SortOrder,
    DateTime? PublishedAt);

public sealed record AdminLocationRowDto(
    int Id,
    string Province,
    string? District,
    string Name,
    string Slug,
    string? CoverImage,
    string Status,
    bool IsFeatured,
    int SortOrder);

public sealed record AdminProjectRowDto(
    int Id,
    string Title,
    string Slug,
    string? ProjectType,
    string? LocationText,
    DateOnly? CompletedAt,
    string? CoverImage,
    string Status,
    bool IsFeatured,
    int SortOrder);

public sealed record AdminFaqListViewModel(
    IReadOnlyList<AdminFaqRowDto> Items,
    string? EntityType,
    string? Query,
    int Page,
    int TotalCount)
{
    public AdminPaginationViewModel Pager => new(Page, 20, TotalCount, (int)Math.Ceiling(TotalCount / 20.0));
}

public sealed record AdminFaqRowDto(
    int Id,
    string Question,
    string EntityType,
    string Status,
    int SortOrder);
