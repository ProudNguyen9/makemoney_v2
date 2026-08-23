using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Areas.Admin.ViewModels.Data;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Data;

namespace ScrapWebsite.Services.Admin;

public sealed class AdminQueryService :
    IAdminDashboardQueryService,
    IAdminScrapQueryService,
    IAdminArticleQueryService,
    IAdminPriceQueryService,
    IAdminSeoQueryService,
    IAdminSettingsQueryService,
    IAdminServiceQueryService,
    IAdminLocationQueryService,
    IAdminProjectQueryService,
    IAdminFaqQueryService
{
    private const int AdminListLimit = 50;
    private const int AdminPageSize = 20;
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

    public async Task<AdminScrapListViewModel> GetScrapListAsync(string? group, string? status, string? query, int page, CancellationToken cancellationToken)
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
            .Skip(Math.Max(0, page - 1) * AdminPageSize)
            .Take(AdminPageSize)
            .ToListAsync(cancellationToken);

        return new AdminScrapListViewModel(categories, items, group, status, query, page, totalCount);
    }

    public async Task<IReadOnlyList<AdminCategoryOptionDto>> GetCategoryOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ScrapCategories.AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new AdminCategoryOptionDto(category.Id, category.Name, category.Slug))
            .ToListAsync(cancellationToken);
    }

    public async Task<ScrapItemFormViewModel?> GetScrapFormAsync(int? id, CancellationToken cancellationToken)
    {
        ScrapItemFormViewModel form;
        if (id is null or 0)
        {
            form = new ScrapItemFormViewModel();
        }
        else
        {
            var item = await _dbContext.ScrapItems.AsNoTracking()
                .Include(scrap => scrap.Prices)
                .Include(scrap => scrap.Images)
                .FirstOrDefaultAsync(scrap => scrap.Id == id.Value, cancellationToken);
            if (item is null)
            {
                return null;
            }

            form = new ScrapItemFormViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Slug = item.Slug,
                CategoryId = item.ScrapCategoryId,
                ShortDescription = item.ShortDescription,
                Description = item.Description,
                PriceLabel = item.PriceLabel,
                Unit = item.Unit ?? "kg",
                Status = item.Status,
                SortOrder = item.SortOrder,
                IsFeatured = item.IsFeatured,
                CurrentThumbUrl = item.PrimaryImage,
                CurrentBannerUrl = item.Images.FirstOrDefault(image => image.Caption == "banner")?.ImageUrl,
                PriceRows = item.Prices
                    .OrderByDescending(price => price.EffectiveDate)
                    .ThenBy(price => price.Id)
                    .Select(price => new ScrapPriceRowInput { Label = price.PriceLabel, PriceValue = price.PriceValue, Unit = price.Unit })
                    .ToList()
            };
        }

        form.Categories = await GetCategoryOptionsAsync(cancellationToken);
        return form;
    }

    public async Task<AdminArticleListViewModel> GetArticleListAsync(string? category, string? status, string? query, CancellationToken cancellationToken)
    {
        var categories = await _dbContext.PostCategories.AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(item => new AdminCategoryOptionDto(item.Id, item.Name, item.Slug))
            .ToListAsync(cancellationToken);

        var showDeleted = string.Equals(status, "deleted", StringComparison.OrdinalIgnoreCase);
        var showFeatured = string.Equals(status, "featured", StringComparison.OrdinalIgnoreCase);
        var baseQuery = _dbContext.Posts.AsNoTracking();
        baseQuery = showDeleted
            ? baseQuery.Where(post => post.DeletedAt != null)
            : baseQuery.Where(post => post.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(category))
        {
            baseQuery = baseQuery.Where(post => post.Category != null && post.Category.Slug == category);
        }

        if (!showDeleted && showFeatured)
        {
            baseQuery = baseQuery.Where(post => post.IsFeatured);
        }
        else if (!showDeleted && !string.IsNullOrWhiteSpace(status))
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

    public async Task<PostFormViewModel?> GetArticleFormAsync(int? id, CancellationToken cancellationToken)
    {
        var categories = await _dbContext.PostCategories.AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(item => new AdminCategoryOptionDto(item.Id, item.Name, item.Slug))
            .ToListAsync(cancellationToken);

        PostFormViewModel form;
        if (id is null || id == 0)
        {
            form = new PostFormViewModel { PublishedAt = DateTime.UtcNow };
        }
        else
        {
            var post = await _dbContext.Posts.AsNoTracking()
                .Include(item => item.ProductLinks)
                .Where(item => item.DeletedAt == null)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (post is null)
            {
                return null;
            }

            form = new PostFormViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                PostCategoryId = post.PostCategoryId,
                Excerpt = post.Excerpt,
                Content = post.Content,
                Status = post.Status,
                PublishedAt = post.PublishedAt,
                SortOrder = post.SortOrder,
                IsFeatured = post.IsFeatured,
                AuthorName = post.AuthorName,
                CurrentCoverUrl = post.CoverImage,
                LinkedProductIds = post.ProductLinks
                    .OrderBy(link => link.SortOrder)
                    .ThenBy(link => link.Id)
                    .Select(link => link.ScrapItemId)
                    .ToList()
            };
        }

        form.Categories = categories;
        form.ProductOptions = await _dbContext.ScrapItems.AsNoTracking()
            .Where(item => item.Status == "published")
            .OrderByDescending(item => item.IsFeatured)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(item => new AdminCategoryOptionDto(item.Id, item.Name, item.Slug))
            .ToListAsync(cancellationToken);
        return form;
    }

    public async Task<AdminPriceListViewModel> GetPriceListAsync(string? group, string? status, string? query, int page, CancellationToken cancellationToken)
    {
        var categories = await _dbContext.ScrapCategories.AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new AdminCategoryOptionDto(category.Id, category.Name, category.Slug))
            .ToListAsync(cancellationToken);

        var baseQuery = _dbContext.ScrapPrices.AsNoTracking().Where(price => price.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(group))
        {
            baseQuery = baseQuery.Where(price => price.ScrapItem != null && price.ScrapItem.Category != null && price.ScrapItem.Category.Slug == group);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery = status == "published"
                ? baseQuery.Where(price => price.ScrapItem != null && price.ScrapItem.Status == "published")
                : baseQuery.Where(price => price.ScrapItem != null && price.ScrapItem.Status != "published");
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
                price.ScrapItemId,
                price.ScrapItem != null ? price.ScrapItem.Name : "Chưa gắn loại",
                price.ScrapItem != null && price.ScrapItem.Category != null ? price.ScrapItem.Category.Name : "Chưa phân nhóm",
                price.PriceValue,
                BuildPriceText(price.PriceLabel, price.PriceValue, price.Unit),
                price.Unit ?? "kg",
                price.EffectiveDate,
                "active",
                price.ScrapItem != null && price.ScrapItem.Status == "published"))
            .Skip(Math.Max(0, page - 1) * AdminPageSize)
            .Take(AdminPageSize)
            .ToListAsync(cancellationToken);

        var lastUpdatedAt = await _dbContext.ScrapPriceHistory.AsNoTracking()
            .OrderByDescending(price => price.EffectiveDate)
            .Select(price => (DateOnly?)price.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        return new AdminPriceListViewModel(categories, items, group, status, query, page, totalCount, lastUpdatedAt);
    }

    // ------------------------------------------------------------------
    // Dịch vụ
    // ------------------------------------------------------------------

    public async Task<AdminServiceListViewModel> GetServiceListAsync(string? status, string? query, int page, CancellationToken cancellationToken)
    {
        var baseQuery = _dbContext.Services.AsNoTracking().Where(service => service.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery = baseQuery.Where(service => service.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(service => service.Title.Contains(query) || service.Slug.Contains(query));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(service => service.SortOrder)
            .ThenBy(service => service.Title)
            .Select(service => new AdminServiceRowDto(
                service.Id,
                service.Title,
                service.Slug,
                service.CoverImage,
                service.IconCss,
                service.Status,
                service.IsFeatured,
                service.SortOrder,
                service.PublishedAt))
            .Skip(Math.Max(0, page - 1) * AdminPageSize)
            .Take(AdminPageSize)
            .ToListAsync(cancellationToken);

        return new AdminServiceListViewModel(items, status, query, page, totalCount);
    }

    public async Task<ServiceFormViewModel?> GetServiceFormAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is null or 0)
        {
            return new ServiceFormViewModel();
        }

        var entity = await _dbContext.Services.AsNoTracking()
            .FirstOrDefaultAsync(service => service.Id == id.Value && service.DeletedAt == null, cancellationToken);
        return entity is null
            ? null
            : new ServiceFormViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Slug = entity.Slug,
                IconCss = entity.IconCss,
                Excerpt = entity.Excerpt,
                ContentHtml = entity.ContentHtml,
                CurrentCoverUrl = entity.CoverImage,
                Status = entity.Status,
                SortOrder = entity.SortOrder,
                IsFeatured = entity.IsFeatured
            };
    }

    // ------------------------------------------------------------------
    // Khu vực
    // ------------------------------------------------------------------

    public async Task<AdminLocationListViewModel> GetLocationListAsync(string? province, string? status, string? query, int page, CancellationToken cancellationToken)
    {
        var provinces = await _dbContext.Locations.AsNoTracking()
            .Where(location => location.DeletedAt == null && location.Province != "")
            .Select(location => location.Province)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);

        var baseQuery = _dbContext.Locations.AsNoTracking().Where(location => location.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(province))
        {
            baseQuery = baseQuery.Where(location => location.Province == province);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery = baseQuery.Where(location => location.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(location => location.Name.Contains(query) || location.Slug.Contains(query) || location.Province.Contains(query));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(location => location.Province)
            .ThenBy(location => location.Name)
            .Select(location => new AdminLocationRowDto(
                location.Id,
                location.Province,
                location.District,
                location.Name,
                location.Slug,
                location.CoverImage,
                location.Status,
                location.IsFeatured,
                location.SortOrder))
            .Skip(Math.Max(0, page - 1) * AdminPageSize)
            .Take(AdminPageSize)
            .ToListAsync(cancellationToken);

        return new AdminLocationListViewModel(provinces, items, province, status, query, page, totalCount);
    }

    public async Task<LocationFormViewModel?> GetLocationFormAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is null or 0)
        {
            return new LocationFormViewModel();
        }

        var entity = await _dbContext.Locations.AsNoTracking()
            .FirstOrDefaultAsync(location => location.Id == id.Value && location.DeletedAt == null, cancellationToken);
        return entity is null
            ? null
            : new LocationFormViewModel
            {
                Id = entity.Id,
                Province = entity.Province,
                District = entity.District,
                Name = entity.Name,
                Slug = entity.Slug,
                Excerpt = entity.Excerpt,
                ContentHtml = entity.ContentHtml,
                CurrentCoverUrl = entity.CoverImage,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Status = entity.Status,
                SortOrder = entity.SortOrder,
                IsFeatured = entity.IsFeatured
            };
    }

    // ------------------------------------------------------------------
    // Dự án
    // ------------------------------------------------------------------

    public async Task<AdminProjectListViewModel> GetProjectListAsync(string? projectType, string? status, string? query, int page, CancellationToken cancellationToken)
    {
        var projectTypes = await _dbContext.Projects.AsNoTracking()
            .Where(project => project.DeletedAt == null && project.ProjectType != null && project.ProjectType != "")
            .Select(project => project.ProjectType!)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);

        var baseQuery = _dbContext.Projects.AsNoTracking().Where(project => project.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(projectType))
        {
            baseQuery = baseQuery.Where(project => project.ProjectType == projectType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery = baseQuery.Where(project => project.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(project => project.Title.Contains(query) || project.Slug.Contains(query) || project.LocationText != null && project.LocationText.Contains(query));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(project => project.SortOrder)
            .ThenBy(project => project.Id)
            .Select(project => new AdminProjectRowDto(
                project.Id,
                project.Title,
                project.Slug,
                project.ProjectType,
                project.LocationText,
                project.CompletedAt,
                project.CoverImage,
                project.Status,
                project.IsFeatured,
                project.SortOrder))
            .Skip(Math.Max(0, page - 1) * AdminPageSize)
            .Take(AdminPageSize)
            .ToListAsync(cancellationToken);

        return new AdminProjectListViewModel(projectTypes, items, projectType, status, query, page, totalCount);
    }

    public async Task<ProjectFormViewModel?> GetProjectFormAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is null or 0)
        {
            return new ProjectFormViewModel();
        }

        var entity = await _dbContext.Projects.AsNoTracking()
            .Include(project => project.Images)
            .FirstOrDefaultAsync(project => project.Id == id.Value && project.DeletedAt == null, cancellationToken);
        return entity is null
            ? null
            : new ProjectFormViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Slug = entity.Slug,
                ProjectType = entity.ProjectType,
                LocationText = entity.LocationText,
                Excerpt = entity.Excerpt,
                ContentHtml = entity.ContentHtml,
                CurrentCoverUrl = entity.CoverImage,
                CompletedAt = entity.CompletedAt,
                QuantityText = entity.QuantityText,
                DurationText = entity.DurationText,
                Status = entity.Status,
                SortOrder = entity.SortOrder,
                IsFeatured = entity.IsFeatured,
                Gallery = entity.Images
                    .OrderBy(image => image.SortOrder)
                    .ThenBy(image => image.Id)
                    .Select(image => new ProjectGalleryRowInput { Id = image.Id, ImageUrl = image.ImageUrl, AltText = image.AltText, SortOrder = image.SortOrder })
                    .ToList()
            };
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

    // ------------------------------------------------------------------
    // FAQ
    // ------------------------------------------------------------------

    public async Task<AdminFaqListViewModel> GetFaqListAsync(string? entityType, string? query, int page, CancellationToken cancellationToken)
    {
        var baseQuery = _dbContext.FaqItems.AsNoTracking().Where(faq => faq.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(entityType))
        {
            baseQuery = baseQuery.Where(faq => faq.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(faq => faq.Question.Contains(query));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(faq => faq.EntityType)
            .ThenBy(faq => faq.SortOrder)
            .ThenBy(faq => faq.Id)
            .Select(faq => new AdminFaqRowDto(
                faq.Id,
                faq.Question,
                faq.EntityType,
                faq.Status,
                faq.SortOrder))
            .Skip(Math.Max(0, page - 1) * AdminPageSize)
            .Take(AdminPageSize)
            .ToListAsync(cancellationToken);

        return new AdminFaqListViewModel(items, entityType, query, page, totalCount);
    }

    public async Task<FaqFormViewModel?> GetFaqFormAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is null or 0)
        {
            return new FaqFormViewModel();
        }

        var entity = await _dbContext.FaqItems.AsNoTracking()
            .FirstOrDefaultAsync(faq => faq.Id == id.Value && faq.DeletedAt == null, cancellationToken);
        return entity is null
            ? null
            : new FaqFormViewModel
            {
                Id = entity.Id,
                Question = entity.Question,
                Answer = entity.Answer,
                EntityType = entity.EntityType,
                Status = entity.Status,
                SortOrder = entity.SortOrder
            };
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
            post.AuthorName,
            post.IsFeatured,
            post.DeletedAt));
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
