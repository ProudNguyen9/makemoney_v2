using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ScrapWebsite.Areas.Admin.ViewModels.Data;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Data;
using ScrapWebsite.Services;

namespace ScrapWebsite.Services.Admin;

public sealed class AdminQueryService :
    IAdminDashboardQueryService,
    IAdminScrapQueryService,
    IAdminArticleQueryService,
    IAdminPriceQueryService,
    IAdminLeadQueryService,
    IAdminSeoQueryService,
    IAdminSettingsQueryService,
    IAdminMediaQueryService,
    IAdminServiceQueryService,
    IAdminLocationQueryService,
    IAdminProjectQueryService,
    IAdminFaqQueryService
{
    private const int AdminListLimit = 50;
    private const int AdminPageSize = 20;
    private readonly AppDbContext _dbContext;
    private readonly SmtpOptions _smtpFallback;

    public AdminQueryService(AppDbContext dbContext, IOptions<SmtpOptions> smtpOptions)
    {
        _dbContext = dbContext;
        _smtpFallback = smtpOptions.Value;
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var scrapCount = await _dbContext.ScrapItems.AsNoTracking()
            .CountAsync(item => item.DeletedAt == null, cancellationToken);
        var postCount = await _dbContext.Posts.AsNoTracking().CountAsync(cancellationToken);
        var mediaCount = await _dbContext.MediaFiles.AsNoTracking().CountAsync(cancellationToken);
        var seoCount = await _dbContext.SeoMetadata.AsNoTracking().CountAsync(cancellationToken);
        var leadCount = await _dbContext.ContactRequests.AsNoTracking()
            .CountAsync(request => request.DeletedAt == null && request.Status != "contacted", cancellationToken);
        var locationCount = await _dbContext.Locations.AsNoTracking()
            .CountAsync(location => location.DeletedAt == null, cancellationToken);
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
                .Where(item => item.DeletedAt == null)
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
            locationCount,
            leadCount,
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

        var baseQuery = _dbContext.ScrapItems.AsNoTracking().Where(item => item.DeletedAt == null);
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
            // SCR-001: mặc định vị trí = cuối danh sách (max + 1) thay vì 0 để nút Lưu không bị chặn.
            var maxSortOrder = await _dbContext.ScrapItems.AsNoTracking()
                .Where(item => item.DeletedAt == null)
                .MaxAsync(item => (int?)item.SortOrder, cancellationToken);
            form.SortOrder = Math.Max(maxSortOrder ?? 0, 0) + 1;
        }
        else
        {
            var item = await _dbContext.ScrapItems.AsNoTracking()
                .Include(scrap => scrap.Prices)
                .Include(scrap => scrap.Images)
                .FirstOrDefaultAsync(scrap => scrap.Id == id.Value && scrap.DeletedAt == null, cancellationToken);
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
                SeoKeywords = item.SeoKeywords,
                CurrentThumbUrl = item.PrimaryImage,
                CurrentBannerUrl = item.Images.FirstOrDefault(image => image.Caption == "banner")?.ImageUrl,
                PriceRows = item.Prices
                    .OrderByDescending(price => price.EffectiveDate)
                    .ThenBy(price => price.Id)
                    .Select(price => new ScrapPriceRowInput { Label = price.PriceLabel, PriceValue = price.PriceValue, Unit = price.Unit })
                    .ToList(),
                Gallery = item.Images
                    .Where(image => image.Caption != "banner")
                    .OrderBy(image => image.OrderIndex)
                    .ThenBy(image => image.Id)
                    .Select(image => new ScrapGalleryRowInput { Id = image.Id, ImageUrl = image.ImageUrl, Caption = image.Caption, OrderIndex = image.OrderIndex })
                    .ToList()
            };
        }

        form.Categories = await GetCategoryOptionsAsync(cancellationToken);
        return form;
    }

    public async Task<AdminScrapCategoryListViewModel> GetScrapCategoryListAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.ScrapCategories.AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Id)
            .Select(category => new AdminScrapCategoryRowDto(
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.Status,
                category.SortOrder,
                category.ScrapItems.Count))
            .ToListAsync(cancellationToken);

        return new AdminScrapCategoryListViewModel(items);
    }

    public async Task<ScrapCategoryFormViewModel?> GetScrapCategoryFormAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is null or 0)
        {
            var maxSortOrder = await _dbContext.ScrapCategories.AsNoTracking()
                .MaxAsync(category => (int?)category.SortOrder, cancellationToken);
            return new ScrapCategoryFormViewModel { SortOrder = Math.Max(maxSortOrder ?? 0, 0) + 1 };
        }

        var entity = await _dbContext.ScrapCategories.AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id.Value, cancellationToken);
        return entity is null
            ? null
            : new ScrapCategoryFormViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                Description = entity.Description,
                SortOrder = entity.SortOrder,
                Status = entity.Status,
                SeoKeywords = entity.SeoKeywords
            };
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
            form = new PostFormViewModel { PublishedAt = DateTime.UtcNow, AutosaveKey = $"new-{Guid.NewGuid():N}" };
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
                SeoKeywords = post.SeoKeywords,
                CurrentCoverUrl = post.CoverImage,
                AutosaveKey = $"post-{post.Id}",
                UpdatedAtUtc = post.UpdatedAt,
                LinkedProductIds = post.ProductLinks
                    .OrderBy(link => link.SortOrder)
                    .ThenBy(link => link.Id)
                    .Select(link => link.ScrapItemId)
                    .ToList()
            };

            // Khôi phục nội dung đang soạn dở (auto-save) nếu mới hơn lần lưu chính thức.
            var autosaveKey = $"post-{post.Id}";
            var autosavedAt = await _dbContext.PostAutosaves.AsNoTracking()
                .Where(item => item.PostKey == autosaveKey)
                .Select(item => (DateTime?)item.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (autosavedAt.HasValue && autosavedAt.Value > post.UpdatedAt.AddSeconds(-2))
            {
                var autosaveJson = await _dbContext.PostAutosaves.AsNoTracking()
                    .Where(item => item.PostKey == autosaveKey)
                    .Select(item => item.DataJson)
                    .FirstOrDefaultAsync(cancellationToken);

                var payload = autosaveJson is null ? null : PostAutosavePayloadMapper.Deserialize(autosaveJson);
                if (payload is not null)
                {
                    // Giữ trạng thái gốc của bài viết, chỉ khôi phục nội dung đang soạn.
                    PostAutosavePayloadMapper.ApplyTo(payload, form);
                    form.AutosavedAtUtc = autosavedAt.Value;
                    form.RestoredFromAutosave = true;
                }
            }
        }

        form.Categories = categories;
        form.ProductOptions = await _dbContext.ScrapItems.AsNoTracking()
            .Include(item => item.Category)
            .Where(item => item.Status == "published" && item.DeletedAt == null)
            .OrderByDescending(item => item.IsFeatured)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(item => new AdminLinkedProductDto(
                item.Id,
                item.Name,
                item.Slug,
                item.Category != null ? item.Category.Name : "Chưa phân loại",
                item.Status,
                item.PrimaryImage,
                item.PriceLabel ?? (item.PriceFrom.HasValue ? $"{item.PriceFrom.Value:N0} đ/{(item.Unit ?? "kg")}" : null),
                item.ShortDescription))
            .ToListAsync(cancellationToken);
        return form;
    }

    public async Task<string?> GetArticleStatusAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Posts.AsNoTracking()
            .Where(post => post.Id == id && post.DeletedAt == null)
            .Select(post => post.Status)
            .FirstOrDefaultAsync(cancellationToken);
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

        // Ẩn dòng giá thuộc phế liệu đã xóa mềm.
        baseQuery = baseQuery.Where(price => price.ScrapItem == null || price.ScrapItem.DeletedAt == null);

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

    public async Task<AdminLeadListViewModel> GetLeadListAsync(string? status, string? scrap, string? area, string? query, int page, CancellationToken cancellationToken)
    {
        status = CleanOptional(status);
        scrap = CleanOptional(scrap);
        area = CleanOptional(area);
        query = CleanOptional(query);

        var baseQuery = _dbContext.ContactRequests
            .AsNoTracking()
            .Include(request => request.Files)
            .Where(request => request.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery = baseQuery.Where(request => request.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(scrap))
        {
            baseQuery = baseQuery.Where(request => request.ScrapType == scrap);
        }

        if (!string.IsNullOrWhiteSpace(area))
        {
            baseQuery = baseQuery.Where(request => request.Area == area);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(request =>
                (request.Name != null && request.Name.Contains(query)) ||
                request.Phone.Contains(query) ||
                (request.Zalo != null && request.Zalo.Contains(query)) ||
                (request.Message != null && request.Message.Contains(query)));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / AdminPageSize));
        page = Math.Clamp(page, 1, totalPages);

        var items = await baseQuery
            .OrderBy(request => request.Status == "contacted")
            .ThenByDescending(request => request.CreatedAt)
            .Skip((page - 1) * AdminPageSize)
            .Take(AdminPageSize)
            .Select(request => new AdminLeadRowDto(
                request.Id,
                $"LE-{request.Id:0000}",
                string.IsNullOrWhiteSpace(request.Name) ? "Khách chưa nhập tên" : request.Name!,
                request.Phone,
                request.Zalo,
                request.ScrapType,
                request.QuantityText,
                request.Area,
                request.Message,
                request.SourceForm,
                request.SourceUrl,
                request.Status,
                request.CreatedAt,
                request.Files.OrderBy(file => file.Id).Select(file => file.FileUrl).ToList()))
            .ToListAsync(cancellationToken);

        var scrapTypes = await _dbContext.ContactRequests.AsNoTracking()
            .Where(request => request.DeletedAt == null && request.ScrapType != null && request.ScrapType != "")
            .Select(request => request.ScrapType!)
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync(cancellationToken);

        var areas = await _dbContext.ContactRequests.AsNoTracking()
            .Where(request => request.DeletedAt == null && request.Area != null && request.Area != "")
            .Select(request => request.Area!)
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync(cancellationToken);

        return new AdminLeadListViewModel(items, scrapTypes, areas, status, scrap, area, query, page, totalCount);
    }

    public async Task<AdminLeadDetailDto?> GetLeadDetailAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.ContactRequests
            .AsNoTracking()
            .Include(request => request.Files)
            .Where(request => request.DeletedAt == null && request.Id == id)
            .Select(request => new AdminLeadDetailDto(
                request.Id,
                $"LE-{request.Id:0000}",
                string.IsNullOrWhiteSpace(request.Name) ? "Khách chưa nhập tên" : request.Name!,
                request.Phone,
                request.Zalo,
                request.Email,
                request.ScrapType,
                request.QuantityText,
                request.Area,
                request.Message,
                request.SourceForm,
                request.SourceUrl,
                request.Status,
                request.CreatedAt,
                request.Files.OrderBy(file => file.Id).Select(file => file.FileUrl).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
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
            var maxSortOrder = await _dbContext.Services.AsNoTracking()
                .Where(service => service.DeletedAt == null)
                .MaxAsync(service => (int?)service.SortOrder, cancellationToken);
            return new ServiceFormViewModel { SortOrder = Math.Max(maxSortOrder ?? 0, 0) + 1 };
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
                IsFeatured = entity.IsFeatured,
                SeoKeywords = entity.SeoKeywords
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
            var maxSortOrder = await _dbContext.Projects.AsNoTracking()
                .Where(project => project.DeletedAt == null)
                .MaxAsync(project => (int?)project.SortOrder, cancellationToken);
            return new ProjectFormViewModel { SortOrder = Math.Max(maxSortOrder ?? 0, 0) + 1 };
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
                SeoKeywords = entity.SeoKeywords,
                Gallery = entity.Images
                    .OrderBy(image => image.SortOrder)
                    .ThenBy(image => image.Id)
                    .Select(image => new ProjectGalleryRowInput { Id = image.Id, ImageUrl = image.ImageUrl, AltText = image.AltText, SortOrder = image.SortOrder })
                    .ToList()
            };
    }

    public async Task<AdminSeoListViewModel> GetSeoListAsync(string? entityType, string? status, string? indexState, string? query, CancellationToken cancellationToken)
    {
        var baseQuery = _dbContext.SeoMetadata.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(entityType))
        {
            baseQuery = baseQuery.Where(seo => seo.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery = baseQuery.Where(seo => seo.Status == status);
        }

        if (indexState == "index")
        {
            baseQuery = baseQuery.Where(seo => seo.RobotsIndex);
        }
        else if (indexState == "noindex")
        {
            baseQuery = baseQuery.Where(seo => !seo.RobotsIndex);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            baseQuery = baseQuery.Where(seo =>
                (seo.RoutePath != null && seo.RoutePath.Contains(query)) ||
                seo.EntityType.Contains(query) ||
                seo.SeoTitle.Contains(query) ||
                (seo.MetaDescription != null && seo.MetaDescription.Contains(query)) ||
                (seo.OgTitle != null && seo.OgTitle.Contains(query)));
        }

        var entityTypes = await _dbContext.SeoMetadata.AsNoTracking()
            .Select(seo => seo.EntityType)
            .Distinct()
            .OrderBy(type => type)
            .ToListAsync(cancellationToken);

        var rows = await baseQuery
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
                seo.OgTitle ?? "",
                seo.OgDescription ?? "",
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
            entityTypes,
            sitemapCount,
            redirectCount,
            Get(settings, "seo.site_title", Get(settings, "site.name", "Phế Liệu Minh Đức")),
            Get(settings, "seo.default_description", "Thu mua phế liệu tận nơi giá cao, cân minh bạch, thanh toán nhanh."),
            Get(settings, "seo.default_og_title", Get(settings, "seo.site_title", Get(settings, "site.name", "Phế Liệu Minh Đức"))),
            Get(settings, "seo.default_og_image", Get(settings, "site.default_og_image", "/assets/images/imported/brand/banner-1.jpg")),
            entityType,
            status,
            indexState,
            query);
    }

    public async Task<AdminSettingsViewModel> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await LoadSettingsAsync(cancellationToken);

        var smtpHost = Get(settings, "smtp.host", _smtpFallback.Host);
        var smtpPortRaw = Get(settings, "smtp.port", _smtpFallback.Port > 0 ? _smtpFallback.Port.ToString() : "587");
        var smtpPort = int.TryParse(smtpPortRaw, out var parsedPort) && parsedPort is > 0 and <= 65535 ? parsedPort : 587;
        var smtpSslRaw = Get(settings, "smtp.enable_ssl", string.Empty);
        var smtpEnableSsl = bool.TryParse(smtpSslRaw, out var sslFlag) ? sslFlag : _smtpFallback.EnableSsl;

        return new AdminSettingsViewModel(
            Get(settings, "site.name", "Phế Liệu Minh Đức"),
            Get(settings, "company.tax_code", "Đang cập nhật"),
            Get(settings, "contact.address", Get(settings, "contact.warehouse_address", "TP.HCM")),
            Get(settings, "contact.phone", "0985565323"),
            Get(settings, "contact.zalo", "0985565323"),
            Get(settings, "contact.email", "phelieuminhduc@gmail.com"),
            Get(settings, "contact.working_hours", "T2-CN: 7:00 - 20:00"),
            Get(settings, "contact.purchase_areas", "TP.HCM, Bình Dương, Đồng Nai"),
            Get(settings, "social.facebook", ""),
            Get(settings, "site.logo", "/assets/images/imported/brand/logo.png"),
            Get(settings, "site.footer_logo", "/assets/images/imported/brand/logo-footer.png"),
            Get(settings, "site.favicon", "/favicon.ico"),
            Get(settings, "home.price_updated_text", DateTime.Today.ToString("dd/MM/yyyy")),
            Get(settings, "home.response_time_text", "30 phút"),
            Get(settings, "system.cache_minutes", "5"),
            smtpHost,
            smtpPort,
            smtpEnableSsl,
            Get(settings, "smtp.username", _smtpFallback.UserName),
            !string.IsNullOrWhiteSpace(settings.GetValueOrDefault("smtp.password")),
            Get(settings, "smtp.from_email", _smtpFallback.FromEmail),
            Get(settings, "smtp.from_name", _smtpFallback.FromName),
            Get(settings, "smtp.to_email", Get(settings, "contact.email", _smtpFallback.ToEmail ?? string.Empty)));
    }

    public async Task<AdminMediaListViewModel> GetMediaListAsync(string? group, string? query, CancellationToken cancellationToken)
    {
        var mediaKeys = AdminMediaCatalog.Items.Select(item => item.Key).ToArray();
        var settings = await _dbContext.SiteSettings
            .AsNoTracking()
            .Where(setting => mediaKeys.Contains(setting.Key))
            .Select(setting => new { setting.Key, setting.Value })
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value ?? string.Empty, cancellationToken);

        var normalizedGroup = string.IsNullOrWhiteSpace(group) ? null : group.Trim();
        var normalizedQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        var items = AdminMediaCatalog.Items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(normalizedGroup))
        {
            items = items.Where(item => string.Equals(item.GroupKey, normalizedGroup, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            items = items.Where(item =>
                item.Key.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                item.Label.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                item.GroupName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase));
        }

        var grouped = items
            .GroupBy(item => new { item.GroupKey, item.GroupName })
            .Select(grouping => new AdminMediaGroupDto(
                grouping.Key.GroupKey,
                grouping.Key.GroupName,
                grouping.Select(item => new AdminMediaItemDto(
                    item.Key,
                    item.GroupKey,
                    item.GroupName,
                    item.Label,
                    item.Description,
                    item.RecommendedSize,
                    Get(settings, item.Key, item.FallbackUrl))).ToList()))
            .ToList();

        var groupOptions = AdminMediaCatalog.Items
            .GroupBy(item => new { item.GroupKey, item.GroupName })
            .Select(grouping => new AdminMediaGroupOptionDto(grouping.Key.GroupKey, grouping.Key.GroupName, grouping.Count()))
            .ToList();

        return new AdminMediaListViewModel(groupOptions, grouped, normalizedGroup, normalizedQuery);
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
            return new FaqFormViewModel
            {
                SortOrder = 1,
                EntityType = "home",
                Status = "published"
            };
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

    private static string? CleanOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
