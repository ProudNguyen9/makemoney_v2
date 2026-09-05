using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Data;
using ScrapWebsite.Helpers;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Media;

namespace ScrapWebsite.Services.Admin;

/// <summary>
/// Write side of the admin area. Follows the AdminQueryService convention:
/// one scoped implementation forwarded to every command interface.
/// </summary>
public sealed class AdminCommandService :
    IAdminPriceCommandService,
    IAdminLeadCommandService,
    IAdminScrapCommandService,
    IAdminServiceCommandService,
    IAdminLocationCommandService,
    IAdminProjectCommandService,
    IAdminFaqCommandService,
    IAdminArticleCommandService,
    IAdminSettingsCommandService,
    IAdminMediaCommandService,
    IAdminSeoCommandService
{
    private const string Published = "published";
    private const string Draft = "draft";
    private const string SiteChromeCacheKey = "public:site-chrome";
    private const string PublicPageContentCacheKey = "public:page-content-settings";

    private readonly AppDbContext _dbContext;
    private readonly IImageUploadService _imageUpload;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdminCommandService> _logger;

    public AdminCommandService(AppDbContext dbContext, IImageUploadService imageUpload, IMemoryCache cache, ILogger<AdminCommandService> logger)
    {
        _dbContext = dbContext;
        _imageUpload = imageUpload;
        _cache = cache;
        _logger = logger;
    }

    // ------------------------------------------------------------------
    // Bảng giá — bulk inline edit
    // ------------------------------------------------------------------

    public async Task<bool> MarkContactedAsync(int id, CancellationToken cancellationToken)
    {
        var lead = await _dbContext.ContactRequests.FirstOrDefaultAsync(request => request.Id == id && request.DeletedAt == null, cancellationToken);
        if (lead is null)
        {
            return false;
        }

        lead.Status = "contacted";
        lead.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> SavePriceBulkAsync(IReadOnlyList<PriceBulkRowInput> rows, CancellationToken cancellationToken)
    {
        // PRI-002: dòng được tick nhưng giá trống/không hợp lệ thì bỏ qua,
        // không bao giờ ghi PriceValue = NULL xuống DB.
        var selectedIds = rows
            .Where(row => row.Selected && row.PriceValue.HasValue)
            .Select(row => row.PriceId)
            .ToList();
        if (selectedIds.Count == 0)
        {
            return 0;
        }

        var prices = await _dbContext.ScrapPrices
            .Where(price => price.DeletedAt == null && selectedIds.Contains(price.Id))
            .ToDictionaryAsync(price => price.Id, cancellationToken);

        var changed = 0;
        var affectedItemIds = new HashSet<int>();
        foreach (var row in rows.Where(row => row.Selected && row.PriceValue.HasValue && prices.ContainsKey(row.PriceId)))
        {
            var price = prices[row.PriceId];
            var valueChanged = price.PriceValue != row.PriceValue;
            var unitChanged = !string.Equals(price.Unit, row.Unit ?? "kg", StringComparison.Ordinal);
            if (!valueChanged && !unitChanged)
            {
                continue;
            }

            price.PriceValue = row.PriceValue;
            price.Unit = string.IsNullOrWhiteSpace(row.Unit) ? "kg" : row.Unit.Trim();
            price.EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow);
            affectedItemIds.Add(price.ScrapItemId);

            _dbContext.ScrapPriceHistory.Add(new ScrapPriceHistory
            {
                ScrapItemId = price.ScrapItemId,
                PriceValue = row.PriceValue,
                PriceUnit = price.Unit,
                PriceType = "manual",
                Note = "Cập nhật từ bảng giá quản trị",
                EffectiveDate = price.EffectiveDate,
                RecordedAt = DateTime.UtcNow
            });

            changed++;
        }

        if (changed > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await SyncItemPriceFromAsync(affectedItemIds, cancellationToken);
        }

        return changed;
    }

    public async Task<bool> DeletePriceAsync(int priceId, CancellationToken cancellationToken)
    {
        var price = await _dbContext.ScrapPrices.FindAsync([priceId], cancellationToken);
        if (price is null || price.DeletedAt != null)
        {
            return false;
        }

        // Soft delete: dòng giá được giữ lại trong DB (DeletedAt) và có thể khôi phục.
        price.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await SyncItemPriceFromAsync([price.ScrapItemId], cancellationToken);
        return true;
    }

    public async Task<int> DeletePriceBulkAsync(IReadOnlyList<PriceBulkRowInput> rows, CancellationToken cancellationToken)
    {
        var selectedIds = rows.Where(row => row.Selected).Select(row => row.PriceId).ToList();
        if (selectedIds.Count == 0)
        {
            return 0;
        }

        var prices = await _dbContext.ScrapPrices
            .Where(price => price.DeletedAt == null && selectedIds.Contains(price.Id))
            .ToListAsync(cancellationToken);
        if (prices.Count == 0)
        {
            return 0;
        }

        var deletedAt = DateTime.UtcNow;
        var affectedItemIds = new HashSet<int>();
        foreach (var price in prices)
        {
            price.DeletedAt = deletedAt;
            affectedItemIds.Add(price.ScrapItemId);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await SyncItemPriceFromAsync(affectedItemIds, cancellationToken);
        return prices.Count;
    }

    /// <summary>
    /// Đồng bộ lại giá tham chiếu (PriceFrom) của các loại phế liệu theo dòng giá thấp nhất chưa xóa.
    /// </summary>
    private async Task SyncItemPriceFromAsync(IEnumerable<int> scrapItemIds, CancellationToken cancellationToken)
    {
        var ids = scrapItemIds.ToList();
        var items = await _dbContext.ScrapItems
            .Where(item => ids.Contains(item.Id) && string.IsNullOrWhiteSpace(item.PriceLabel))
            .ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            var hasValue = await _dbContext.ScrapPrices.AsNoTracking()
                .AnyAsync(price => price.DeletedAt == null && price.ScrapItemId == item.Id && price.PriceValue != null, cancellationToken);
            item.PriceFrom = hasValue
                ? await _dbContext.ScrapPrices.AsNoTracking()
                    .Where(price => price.DeletedAt == null && price.ScrapItemId == item.Id && price.PriceValue != null)
                    .MinAsync(price => price.PriceValue!.Value, cancellationToken)
                : null;
            item.UpdatedAt = DateTime.UtcNow;
        }

        if (items.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    // ------------------------------------------------------------------
    // Loại phế liệu
    // ------------------------------------------------------------------

    public async Task<int> SaveScrapItemAsync(ScrapItemFormViewModel form, CancellationToken cancellationToken)
    {
        var slug = await EnsureUniqueSlugAsync(
            _dbContext.ScrapItems.AsNoTracking(),
            SlugHelper.ToSlug(string.IsNullOrWhiteSpace(form.Slug) ? form.Name : form.Slug),
            form.Id,
            filterDeleted: true,
            cancellationToken);

        ScrapItem item;
        if (form.Id == 0)
        {
            // ScrapItems.Id is not an IDENTITY column in the local database (seeded with explicit ids),
            // so the next id is allocated here.
            var nextId = await _dbContext.ScrapItems.AsNoTracking().MaxAsync(scrap => (int?)scrap.Id, cancellationToken) + 1 ?? 1;
            item = new ScrapItem { Id = nextId, CreatedAt = DateTime.UtcNow };
            _dbContext.ScrapItems.Add(item);
        }
        else
        {
            item = await _dbContext.ScrapItems
                .Include(scrap => scrap.Prices)
                .Include(scrap => scrap.Images)
                .FirstOrDefaultAsync(scrap => scrap.Id == form.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy loại phế liệu #{form.Id}.");
        }

        item.Name = form.Name.Trim();
        item.Slug = slug;
        item.ScrapCategoryId = form.CategoryId;
        item.ShortDescription = CleanOptional(form.ShortDescription);
        item.Description = CleanOptional(form.Description);
        item.PriceLabel = CleanOptional(form.PriceLabel);
        item.Unit = string.IsNullOrWhiteSpace(form.Unit) ? "kg" : form.Unit.Trim();
        item.Status = form.Status == Draft ? Draft : Published;
        item.SortOrder = form.SortOrder;
        item.IsFeatured = form.IsFeatured;
        item.SeoKeywords = CleanOptional(form.SeoKeywords);
        item.UpdatedAt = DateTime.UtcNow;
        if (item.Status == Published && item.PublishedAt is null)
        {
            item.PublishedAt = DateTime.UtcNow;
        }

        if (form.ThumbFile is not null)
        {
            var upload = await _imageUpload.SaveAsWebpAsync(form.ThumbFile, "scrap", item.Slug, 800, cancellationToken);
            if (!upload.Success)
            {
                throw new InvalidOperationException(upload.Error);
            }

            await _imageUpload.DeleteUploadedImageAsync(item.PrimaryImage, cancellationToken);
            item.PrimaryImage = upload.Url;
        }
        else if (form.RemoveThumb)
        {
            await _imageUpload.DeleteUploadedImageAsync(item.PrimaryImage, cancellationToken);
            item.PrimaryImage = null;
        }

        if (form.BannerFile is not null)
        {
            var upload = await _imageUpload.SaveAsWebpAsync(form.BannerFile, "scrap", item.Slug + "-banner", 1600, cancellationToken);
            if (!upload.Success)
            {
                throw new InvalidOperationException(upload.Error);
            }

            await RemoveScrapBannerAsync(item, cancellationToken);
            item.Images.Add(new ScrapImage { ImageUrl = upload.Url!, Caption = "banner", OrderIndex = 0 });
        }
        else if (form.RemoveBanner)
        {
            await RemoveScrapBannerAsync(item, cancellationToken);
        }

        // Gallery images: sync existing rows (removed rows from table are deleted on Save).
        var remainingGalleryIds = form.Gallery.Select(row => row.Id).ToHashSet();
        var existingGalleryImages = item.Images.Where(image => image.Caption != "banner").ToList();

        // 1. Delete any image that was removed from the table
        foreach (var image in existingGalleryImages.Where(image => !remainingGalleryIds.Contains(image.Id)))
        {
            await _imageUpload.DeleteUploadedImageAsync(image.ImageUrl, cancellationToken);
            _dbContext.ScrapImages.Remove(image);
        }

        // 2. Update caption and order for remaining rows
        for (var i = 0; i < form.Gallery.Count; i++)
        {
            var row = form.Gallery[i];
            var image = existingGalleryImages.FirstOrDefault(img => img.Id == row.Id);
            if (image != null)
            {
                image.Caption = CleanOptional(row.Caption);
                image.OrderIndex = i;
            }
        }

        // 3. Newly uploaded gallery files.
        var nextGalleryOrder = form.Gallery.Count;
        foreach (var file in form.GalleryFiles.Where(file => file is { Length: > 0 }))
        {
            var upload = await _imageUpload.SaveAsWebpAsync(file, "scrap", item.Slug, 1200, cancellationToken);
            if (!upload.Success)
            {
                _logger.LogWarning("Bỏ qua ảnh gallery phế liệu không hợp lệ: {Error}", upload.Error);
                continue;
            }

            item.Images.Add(new ScrapImage { ImageUrl = upload.Url!, Caption = null, OrderIndex = nextGalleryOrder++ });
        }

        _dbContext.ScrapPrices.RemoveRange(item.Prices);
        var effectiveDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var rowCount = 0;
        foreach (var row in form.PriceRows.Where(row => !string.IsNullOrWhiteSpace(row.Label) || row.PriceValue.HasValue))
        {
            item.Prices.Add(new ScrapPrice
            {
                PriceLabel = CleanOptional(row.Label),
                PriceValue = row.PriceValue,
                Unit = string.IsNullOrWhiteSpace(row.Unit) ? "kg" : row.Unit.Trim(),
                EffectiveDate = effectiveDate
            });
            rowCount++;
        }

        if (rowCount > 0 && string.IsNullOrWhiteSpace(item.PriceLabel))
        {
            item.PriceFrom = form.PriceRows.Where(row => row.PriceValue.HasValue).Select(row => row.PriceValue).Min();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        // Chèn vào đúng vị trí yêu cầu và đánh lại số liên tục 1..n (không bao giờ trùng).
        await RenumberSortAsync(_dbContext.ScrapItems, item.Id, form.SortOrder, cancellationToken);
        return item.Id;
    }

    Task<bool> IAdminScrapCommandService.ToggleStatusAsync(int id, CancellationToken cancellationToken)
        => ToggleAsync(_dbContext.ScrapItems, id, cancellationToken);

    Task<bool> IAdminScrapCommandService.ToggleFeaturedAsync(int id, CancellationToken cancellationToken)
        => ToggleFeaturedAsync(_dbContext.ScrapItems, id, cancellationToken);

    Task<bool> IAdminScrapCommandService.UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken)
        => RenumberSortAsync(_dbContext.ScrapItems, id, sortOrder, cancellationToken);

    public async Task<bool> DeleteScrapItemAsync(int id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.ScrapItems
            .FirstOrDefaultAsync(scrap => scrap.Id == id, cancellationToken);
        if (item is null || item.DeletedAt != null)
        {
            return false;
        }

        // SCR-007: xóa mềm — chỉ ẩn khỏi danh sách và website, dữ liệu giá/ảnh giữ nguyên để khôi phục được.
        item.DeletedAt = DateTime.UtcNow;
        item.Status = Draft;
        item.IsFeatured = false;
        item.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ------------------------------------------------------------------
    // Nhóm phế liệu
    // ------------------------------------------------------------------

    public async Task<int> SaveScrapCategoryAsync(ScrapCategoryFormViewModel form, CancellationToken cancellationToken)
    {
        var name = form.Name.Trim();
        var baseSlug = SlugHelper.ToSlug(string.IsNullOrWhiteSpace(form.Slug) ? name : form.Slug);

        // Slug duy nhất trong phạm vi nhóm chưa xóa.
        var candidate = baseSlug;
        var suffix = 2;
        while (await _dbContext.ScrapCategories.AsNoTracking()
                   .AnyAsync(category => category.Slug == candidate && category.Id != form.Id, cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }

        ScrapCategory entity;
        if (form.Id == 0)
        {
            // ScrapCategories.Id is not an IDENTITY column in the local database (seeded with explicit ids),
            // so the next id is allocated here.
            var nextId = await _dbContext.ScrapCategories.AsNoTracking().MaxAsync(category => (int?)category.Id, cancellationToken) + 1 ?? 1;
            entity = new ScrapCategory { Id = nextId };
            _dbContext.ScrapCategories.Add(entity);
        }
        else
        {
            entity = await _dbContext.ScrapCategories.FirstOrDefaultAsync(category => category.Id == form.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy nhóm phế liệu #{form.Id}.");
        }

        entity.Name = name;
        entity.Slug = candidate;
        entity.Description = CleanOptional(form.Description);
        entity.Status = form.Status == Draft ? Draft : Published;
        entity.SeoKeywords = CleanOptional(form.SeoKeywords);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await RenumberSortAsync(_dbContext.ScrapCategories, entity.Id, form.SortOrder, cancellationToken);
        return entity.Id;
    }

    Task<bool> IAdminScrapCommandService.ToggleCategoryStatusAsync(int id, CancellationToken cancellationToken)
        => ToggleAsync(_dbContext.ScrapCategories, id, cancellationToken);

    public async Task<bool> DeleteScrapCategoryAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _dbContext.ScrapCategories
            .Include(item => item.ScrapItems)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        if (category.ScrapItems.Any())
        {
            throw new InvalidOperationException(
                $"Nhóm \"{category.Name}\" còn {category.ScrapItems.Count} loại phế liệu, không thể xóa. Hãy chuyển hoặc xóa hết phế liệu trong nhóm trước.");
        }

        _dbContext.ScrapCategories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ------------------------------------------------------------------
    // Dịch vụ
    // ------------------------------------------------------------------

    public async Task<int> SaveServiceAsync(ServiceFormViewModel form, CancellationToken cancellationToken)
    {
        var slug = await EnsureUniqueSlugAsync(
            _dbContext.Services.AsNoTracking().Where(service => service.DeletedAt == null),
            SlugHelper.ToSlug(string.IsNullOrWhiteSpace(form.Slug) ? form.Title : form.Slug),
            form.Id,
            filterDeleted: true,
            cancellationToken);

        Service entity;
        if (form.Id == 0)
        {
            entity = new Service { CreatedAt = DateTime.UtcNow };
            _dbContext.Services.Add(entity);
        }
        else
        {
            entity = await _dbContext.Services.FirstOrDefaultAsync(service => service.Id == form.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy dịch vụ #{form.Id}.");
        }

        entity.Title = form.Title.Trim();
        entity.Slug = slug;
        entity.IconCss = CleanOptional(form.IconCss);
        entity.Excerpt = CleanOptional(form.Excerpt);
        entity.ContentHtml = CleanOptional(form.ContentHtml);
        entity.Status = form.Status == Draft ? Draft : Published;
        entity.SortOrder = form.SortOrder;
        entity.IsFeatured = form.IsFeatured;
        entity.SeoKeywords = CleanOptional(form.SeoKeywords);
        entity.UpdatedAt = DateTime.UtcNow;
        if (entity.Status == Published && entity.PublishedAt is null)
        {
            entity.PublishedAt = DateTime.UtcNow;
        }

        if (form.CoverFile is not null)
        {
            var upload = await _imageUpload.SaveAsWebpAsync(form.CoverFile, "service", entity.Slug, 1200, cancellationToken);
            if (!upload.Success)
            {
                throw new InvalidOperationException(upload.Error);
            }

            await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
            entity.CoverImage = upload.Url;
        }
        else if (form.RemoveCover)
        {
            await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
            entity.CoverImage = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        // Chèn vào đúng vị trí yêu cầu và đánh lại số liên tục 1..n (không bao giờ trùng).
        await RenumberSortAsync(_dbContext.Services, entity.Id, form.SortOrder, cancellationToken);
        return entity.Id;
    }

    Task<bool> IAdminServiceCommandService.ToggleStatusAsync(int id, CancellationToken cancellationToken)
        => ToggleAsync(_dbContext.Services, id, cancellationToken);

    Task<bool> IAdminServiceCommandService.ToggleFeaturedAsync(int id, CancellationToken cancellationToken)
        => ToggleFeaturedAsync(_dbContext.Services, id, cancellationToken);

    Task<bool> IAdminServiceCommandService.UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken)
        => RenumberSortAsync(_dbContext.Services, id, sortOrder, cancellationToken);

    public async Task<bool> DeleteServiceAsync(int id, CancellationToken cancellationToken)
        => await SoftDeleteAsync(_dbContext.Services, id, cancellationToken);

    // ------------------------------------------------------------------
    // Khu vực
    // ------------------------------------------------------------------

    public async Task<int> SaveLocationAsync(LocationFormViewModel form, CancellationToken cancellationToken)
    {
        var slug = await EnsureUniqueSlugAsync(
            _dbContext.Locations.AsNoTracking().Where(location => location.DeletedAt == null),
            SlugHelper.ToSlug(string.IsNullOrWhiteSpace(form.Slug)
                ? $"{form.Province} {form.District} {form.Name}"
                : form.Slug),
            form.Id,
            filterDeleted: true,
            cancellationToken);

        Location entity;
        if (form.Id == 0)
        {
            entity = new Location { CreatedAt = DateTime.UtcNow };
            _dbContext.Locations.Add(entity);
        }
        else
        {
            entity = await _dbContext.Locations.FirstOrDefaultAsync(location => location.Id == form.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy khu vực #{form.Id}.");
        }

        entity.Province = form.Province.Trim();
        entity.District = CleanOptional(form.District);
        entity.Name = form.Name.Trim();
        entity.Slug = slug;
        entity.Excerpt = CleanOptional(form.Excerpt);
        entity.ContentHtml = CleanOptional(form.ContentHtml);
        entity.Latitude = form.Latitude;
        entity.Longitude = form.Longitude;
        entity.Status = form.Status == Draft ? Draft : Published;
        entity.SortOrder = form.SortOrder;
        entity.IsFeatured = form.IsFeatured;
        entity.UpdatedAt = DateTime.UtcNow;
        if (entity.Status == Published && entity.PublishedAt is null)
        {
            entity.PublishedAt = DateTime.UtcNow;
        }

        if (form.CoverFile is not null)
        {
            var upload = await _imageUpload.SaveAsWebpAsync(form.CoverFile, "location", entity.Slug, 1200, cancellationToken);
            if (!upload.Success)
            {
                throw new InvalidOperationException(upload.Error);
            }

            await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
            entity.CoverImage = upload.Url;
        }
        else if (form.RemoveCover)
        {
            await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
            entity.CoverImage = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    Task<bool> IAdminLocationCommandService.ToggleStatusAsync(int id, CancellationToken cancellationToken)
        => ToggleAsync(_dbContext.Locations, id, cancellationToken);

    Task<bool> IAdminLocationCommandService.ToggleFeaturedAsync(int id, CancellationToken cancellationToken)
        => ToggleFeaturedAsync(_dbContext.Locations, id, cancellationToken);

    Task<bool> IAdminLocationCommandService.UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken)
        => RenumberSortAsync(_dbContext.Locations, id, sortOrder, cancellationToken);

    public async Task<bool> DeleteLocationAsync(int id, CancellationToken cancellationToken)
        => await SoftDeleteAsync(_dbContext.Locations, id, cancellationToken);

    // ------------------------------------------------------------------
    // Dự án
    // ------------------------------------------------------------------

    public async Task<int> SaveProjectAsync(ProjectFormViewModel form, CancellationToken cancellationToken)
    {
        var slug = await EnsureUniqueSlugAsync(
            _dbContext.Projects.AsNoTracking().Where(project => project.DeletedAt == null),
            SlugHelper.ToSlug(string.IsNullOrWhiteSpace(form.Slug) ? form.Title : form.Slug),
            form.Id,
            filterDeleted: true,
            cancellationToken);

        Project entity;
        if (form.Id == 0)
        {
            entity = new Project { CreatedAt = DateTime.UtcNow };
            _dbContext.Projects.Add(entity);
        }
        else
        {
            entity = await _dbContext.Projects
                .Include(project => project.Images)
                .FirstOrDefaultAsync(project => project.Id == form.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy dự án #{form.Id}.");
        }

        entity.Title = form.Title.Trim();
        entity.Slug = slug;
        entity.ProjectType = CleanOptional(form.ProjectType);
        entity.LocationText = CleanOptional(form.LocationText);
        entity.Excerpt = CleanOptional(form.Excerpt);
        entity.ContentHtml = CleanOptional(form.ContentHtml);
        entity.CompletedAt = form.CompletedAt;
        entity.QuantityText = CleanOptional(form.QuantityText);
        entity.DurationText = CleanOptional(form.DurationText);
        entity.Status = form.Status == Draft ? Draft : Published;
        entity.SortOrder = form.SortOrder;
        entity.IsFeatured = form.IsFeatured;
        entity.SeoKeywords = CleanOptional(form.SeoKeywords);
        entity.UpdatedAt = DateTime.UtcNow;
        if (entity.Status == Published && entity.PublishedAt is null)
        {
            entity.PublishedAt = DateTime.UtcNow;
        }

        if (form.CoverFile is not null)
        {
            var upload = await _imageUpload.SaveAsWebpAsync(form.CoverFile, "project", entity.Slug, 1200, cancellationToken);
            if (!upload.Success)
            {
                throw new InvalidOperationException(upload.Error);
            }

            await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
            entity.CoverImage = upload.Url;
        }
        else if (form.RemoveCover)
        {
            await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
            entity.CoverImage = null;
        }

        // Existing gallery rows: apply alt/sort/remove.
        var existingIds = form.Gallery.Select(row => row.Id).ToList();
        foreach (var image in entity.Images.Where(image => existingIds.Contains(image.Id)).ToList())
        {
            var row = form.Gallery.First(input => input.Id == image.Id);
            if (row.Remove)
            {
                await _imageUpload.DeleteUploadedImageAsync(image.ImageUrl, cancellationToken);
                _dbContext.ProjectImages.Remove(image);
                continue;
            }

            image.AltText = CleanOptional(row.AltText);
            image.SortOrder = row.SortOrder;
        }

        // Newly uploaded gallery files.
        var nextOrder = entity.Images.Count == 0 ? 0 : entity.Images.Max(image => image.SortOrder) + 1;
        foreach (var file in form.GalleryFiles.Where(file => file is { Length: > 0 }))
        {
            var upload = await _imageUpload.SaveAsWebpAsync(file, "project", entity.Slug, 1600, cancellationToken);
            if (!upload.Success)
            {
                _logger.LogWarning("Bỏ qua ảnh gallery không hợp lệ: {Error}", upload.Error);
                continue;
            }

            entity.Images.Add(new ProjectImage { ImageUrl = upload.Url!, AltText = entity.Title, SortOrder = nextOrder++ });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        // Chèn vào đúng vị trí yêu cầu và đánh lại số liên tục 1..n (không bao giờ trùng).
        await RenumberSortAsync(_dbContext.Projects, entity.Id, form.SortOrder, cancellationToken);
        return entity.Id;
    }

    Task<bool> IAdminProjectCommandService.ToggleStatusAsync(int id, CancellationToken cancellationToken)
        => ToggleAsync(_dbContext.Projects, id, cancellationToken);

    Task<bool> IAdminProjectCommandService.ToggleFeaturedAsync(int id, CancellationToken cancellationToken)
        => ToggleFeaturedAsync(_dbContext.Projects, id, cancellationToken);

    Task<bool> IAdminProjectCommandService.UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken)
        => RenumberSortAsync(_dbContext.Projects, id, sortOrder, cancellationToken);

    public async Task<bool> DeleteProjectAsync(int id, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .Include(item => item.Images)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (project is null)
        {
            return false;
        }

        await _imageUpload.DeleteUploadedImageAsync(project.CoverImage, cancellationToken);
        foreach (var image in project.Images)
        {
            await _imageUpload.DeleteUploadedImageAsync(image.ImageUrl, cancellationToken);
        }

        project.DeletedAt = DateTime.UtcNow;
        project.Status = Draft;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ------------------------------------------------------------------
    // FAQ
    // ------------------------------------------------------------------

    public async Task<int> SaveFaqAsync(FaqFormViewModel form, CancellationToken cancellationToken)
    {
        FaqItem entity;
        if (form.Id == 0)
        {
            entity = new FaqItem { CreatedAt = DateTime.UtcNow };
            _dbContext.FaqItems.Add(entity);
        }
        else
        {
            entity = await _dbContext.FaqItems.FirstOrDefaultAsync(faq => faq.Id == form.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy câu hỏi #{form.Id}.");
        }

        entity.EntityType = form.EntityType.Trim();
        entity.Question = form.Question.Trim();
        entity.Answer = form.Answer.Trim();
        entity.Status = form.Status == Draft ? Draft : Published;
        entity.UpdatedAt = DateTime.UtcNow;
        if (entity.Status == Published && entity.PublishedAt is null)
        {
            entity.PublishedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        // Vị trí áp dụng trong nhóm trang được gán, đánh lại số liên tục 1..n.
        await RenumberFaqSortAsync(entity.Id, form.SortOrder, cancellationToken);
        return entity.Id;
    }

    Task<bool> IAdminFaqCommandService.ToggleStatusAsync(int id, CancellationToken cancellationToken)
        => ToggleAsync(_dbContext.FaqItems, id, cancellationToken);

    Task<bool> IAdminFaqCommandService.UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken)
        => RenumberFaqSortAsync(id, sortOrder, cancellationToken);

    public async Task<bool> DeleteFaqAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.FaqItems.FindAsync([id], cancellationToken);
        if (entity is null || entity.DeletedAt != null)
        {
            return false;
        }

        // Xóa mềm — câu hỏi vẫn nằm trong DB và có thể khôi phục.
        entity.DeletedAt = DateTime.UtcNow;
        entity.Status = Draft;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Đánh lại số liên tục cho các câu còn lại trong cùng nhóm trang.
        var siblings = await _dbContext.FaqItems
            .Where(faq => faq.EntityType == entity.EntityType && faq.DeletedAt == null)
            .OrderBy(faq => faq.SortOrder)
            .ThenBy(faq => faq.Id)
            .ToListAsync(cancellationToken);
        var order = 1;
        foreach (var item in siblings)
        {
            _dbContext.FaqItems.Entry(item).Property("SortOrder").CurrentValue = order++;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Đặt FAQ vào vị trí mong muốn trong nhóm trang (EntityType) của nó rồi đánh lại số liên tục 1..n.
    /// </summary>
    private async Task<bool> RenumberFaqSortAsync(int id, int position, CancellationToken cancellationToken)
    {
        var target = await _dbContext.FaqItems.FirstOrDefaultAsync(faq => faq.Id == id, cancellationToken);
        if (target is null)
        {
            return false;
        }

        if (position < 1)
        {
            position = 1;
        }

        var items = await _dbContext.FaqItems
            .Where(faq => faq.EntityType == target.EntityType && faq.DeletedAt == null)
            .OrderBy(faq => faq.SortOrder)
            .ThenBy(faq => faq.Id)
            .ToListAsync(cancellationToken);
        items.Remove(target);
        var index = Math.Clamp(position - 1, 0, items.Count);
        items.Insert(index, target);

        var order = 1;
        foreach (var item in items)
        {
            _dbContext.FaqItems.Entry(item).Property("SortOrder").CurrentValue = order++;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ------------------------------------------------------------------
    // Cài đặt / ảnh thương hiệu
    // ------------------------------------------------------------------

    public async Task SaveSeoMetadataAsync(SeoMetadataFormViewModel form, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.SeoMetadata.FirstOrDefaultAsync(seo => seo.Id == form.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Không tìm thấy SEO #{form.Id}.");

        entity.SeoTitle = form.SeoTitle.Trim();
        entity.MetaDescription = CleanOptional(form.MetaDescription);
        entity.OgTitle = CleanOptional(form.OgTitle);
        entity.OgDescription = CleanOptional(form.OgDescription);
        entity.OgImage = CleanOptional(form.OgImage);
        entity.RobotsIndex = form.RobotsIndex;
        entity.RobotsFollow = form.RobotsFollow;
        entity.Status = form.Status == "inactive" ? "inactive" : "active";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveSeoSiteSettingsAsync(SeoSiteSettingsFormViewModel form, CancellationToken cancellationToken)
    {
        await UpsertSettingAsync("seo.site_title", form.SiteTitle, "seo", "Site title", cancellationToken);
        await UpsertSettingAsync("seo.default_description", form.DefaultDescription, "seo", "Meta description mặc định", cancellationToken);
        await UpsertSettingAsync("seo.default_og_title", form.DefaultOgTitle, "seo", "Tiêu đề khi chia sẻ link", cancellationToken);
        if (form.DefaultOgImageFile is not null)
        {
            await SaveImageSettingAsync("seo.default_og_image", form.DefaultOgImageFile, "og-image", 1200, "seo", "Ảnh OG mặc định", cancellationToken);
        }
        else
        {
            await UpsertSettingAsync("seo.default_og_image", form.DefaultOgImage, "seo", "Ảnh OG mặc định", cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveCompanySettingsAsync(CompanySettingsFormViewModel form, CancellationToken cancellationToken)
    {
        await UpsertSettingAsync("site.name", form.CompanyName, "general", "Tên công ty", cancellationToken);
        await UpsertSettingAsync("company.tax_code", form.TaxCode, "company", "Mã số thuế", cancellationToken);
        await UpsertSettingAsync("contact.address", form.Address, "contact", "Địa chỉ", cancellationToken);
        await UpsertSettingAsync("contact.phone", form.Hotline, "contact", "Hotline", cancellationToken);
        await UpsertSettingAsync("contact.zalo", form.Zalo, "contact", "Zalo", cancellationToken);
        await UpsertSettingAsync("contact.email", form.Email, "contact", "Email", cancellationToken);
        await UpsertSettingAsync("contact.working_hours", form.WorkingHours, "contact", "Giờ làm việc", cancellationToken);
        await UpsertSettingAsync("contact.purchase_areas", form.PurchaseAreas, "contact", "Khu vực thu mua", cancellationToken);
        await UpsertSettingAsync("social.facebook", form.Facebook, "social", "Messenger/Facebook", cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        _cache.Remove(SiteChromeCacheKey);
    }

    public async Task SaveHomepageSettingsAsync(HomepageSettingsFormViewModel form, CancellationToken cancellationToken)
    {
        await UpsertSettingAsync("home.price_updated_text", form.PriceUpdatedText, "home", "Ngày cập nhật bảng giá trang chủ", cancellationToken);
        await UpsertSettingAsync("home.response_time_text", form.ResponseTimeText, "home", "Thời gian phản hồi báo giá", cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        _cache.Remove(SiteChromeCacheKey);
    }

    public async Task SaveBrandAssetsAsync(BrandAssetsFormViewModel form, CancellationToken cancellationToken)
    {
        var changed = false;

        if (form.LogoFile is not null)
        {
            await SaveImageSettingAsync("site.logo", form.LogoFile, "logo", 1200, "site", "Logo chính", cancellationToken);
            changed = true;
        }

        if (form.FooterLogoFile is not null)
        {
            await SaveImageSettingAsync("site.footer_logo", form.FooterLogoFile, "logo-footer", 1200, "site", "Logo footer", cancellationToken);
            changed = true;
        }

        if (!changed)
        {
            throw new InvalidOperationException("Vui lòng chọn ít nhất một ảnh để cập nhật.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _cache.Remove(SiteChromeCacheKey);
    }

    public async Task SaveFaviconAsync(FaviconFormViewModel form, CancellationToken cancellationToken)
    {
        if (form.FaviconFile is null)
        {
            throw new InvalidOperationException("Vui lòng chọn ảnh favicon.");
        }

        await SaveImageSettingAsync("site.favicon", form.FaviconFile, "favicon", 512, "site", "Favicon", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _cache.Remove(SiteChromeCacheKey);
    }

    public async Task SaveSmtpSettingsAsync(SmtpSettingsFormViewModel form, CancellationToken cancellationToken)
    {
        var host = form.Host.Trim();
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("Vui lòng nhập máy chủ SMTP (Host).");
        }

        if (form.Port is < 1 or > 65535)
        {
            throw new InvalidOperationException("Port SMTP phải nằm trong khoảng 1 - 65535.");
        }

        var fromEmail = form.FromEmail.Trim();
        var toEmail = form.ToEmail.Trim();
        if (!IsValidEmail(fromEmail) || !IsValidEmail(toEmail))
        {
            throw new InvalidOperationException("Địa chỉ email người gửi / người nhận không hợp lệ.");
        }

        await UpsertSettingAsync("smtp.host", host, "smtp", "Máy chủ gửi email", cancellationToken);
        await UpsertSettingAsync("smtp.port", form.Port.ToString(), "smtp", "Cổng SMTP", cancellationToken);
        await UpsertSettingAsync("smtp.enable_ssl", form.EnableSsl ? "true" : "false", "smtp", "Bật SSL/TLS", cancellationToken);
        await UpsertSettingAsync("smtp.username", form.UserName, "smtp", "Tên đăng nhập SMTP", cancellationToken);
        await UpsertSettingAsync("smtp.from_email", fromEmail, "smtp", "Email người gửi", cancellationToken);
        await UpsertSettingAsync("smtp.from_name", form.FromName, "smtp", "Tên hiển thị người gửi", cancellationToken);
        await UpsertSettingAsync("smtp.to_email", toEmail, "smtp", "Email nhận thông báo liên hệ", cancellationToken);

        // Mật khẩu để trống nghĩa là giữ nguyên mật khẩu hiện có (không cho xem lại mật khẩu trên UI).
        var existingPassword = await _dbContext.SiteSettings.AsNoTracking()
            .Where(setting => setting.Key == "smtp.password")
            .Select(setting => setting.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(form.Password))
        {
            await UpsertSettingAsync("smtp.password", form.Password.Trim(), "smtp", "Mật khẩu SMTP", cancellationToken);
        }
        else if (existingPassword is null)
        {
            // Chưa từng lưu mật khẩu trong DB: lưu giá trị rỗng để ghi đè fallback appsettings.
            await UpsertSettingAsync("smtp.password", string.Empty, "smtp", "Mật khẩu SMTP", cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _cache.Remove(SmtpSettingsProvider.CacheKey);
    }

    public async Task SaveMediaSettingImageAsync(MediaSettingImageFormViewModel form, CancellationToken cancellationToken)
    {
        var catalogItem = AdminMediaCatalog.Find(form.Key)
            ?? throw new InvalidOperationException("Ảnh không nằm trong danh sách Media được phép sửa.");

        if (form.ImageFile is null)
        {
            throw new InvalidOperationException("Vui lòng chọn ảnh cần tải lên.");
        }

        var setting = await _dbContext.SiteSettings.FirstOrDefaultAsync(item => item.Key == catalogItem.Key, cancellationToken);
        var oldUrl = setting?.Value;
        var upload = await _imageUpload.SaveAsWebpAsync(form.ImageFile, catalogItem.Folder, catalogItem.NameHint, catalogItem.MaxWidth, cancellationToken);
        if (!upload.Success)
        {
            throw new InvalidOperationException(upload.Error);
        }

        if (setting is null)
        {
            setting = new SiteSetting
            {
                Key = catalogItem.Key,
                Group = catalogItem.GroupKey.StartsWith("home", StringComparison.OrdinalIgnoreCase) ? "home" : catalogItem.GroupKey,
                Description = catalogItem.Description
            };
            _dbContext.SiteSettings.Add(setting);
        }
        else
        {
            setting.Description = catalogItem.Description;
        }

        setting.Value = upload.Url;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _imageUpload.DeleteUploadedImageAsync(oldUrl, cancellationToken);
        _cache.Remove(SiteChromeCacheKey);
        _cache.Remove(PublicPageContentCacheKey);
    }

    private async Task SaveImageSettingAsync(
        string key,
        IFormFile file,
        string nameHint,
        int maxWidth,
        string group,
        string description,
        CancellationToken cancellationToken)
    {
        var setting = await _dbContext.SiteSettings.FirstOrDefaultAsync(item => item.Key == key, cancellationToken);
        var oldUrl = setting?.Value;
        var upload = await _imageUpload.SaveAsWebpAsync(file, "brand", nameHint, maxWidth, cancellationToken);
        if (!upload.Success)
        {
            throw new InvalidOperationException(upload.Error);
        }

        if (setting is null)
        {
            setting = new SiteSetting
            {
                Key = key,
                Group = group,
                Description = description
            };
            _dbContext.SiteSettings.Add(setting);
        }

        setting.Value = upload.Url;
        await _imageUpload.DeleteUploadedImageAsync(oldUrl, cancellationToken);
    }

    private async Task UpsertSettingAsync(string key, string? value, string group, string description, CancellationToken cancellationToken)
    {
        var setting = await _dbContext.SiteSettings.FirstOrDefaultAsync(item => item.Key == key, cancellationToken);
        if (setting is null)
        {
            setting = new SiteSetting
            {
                Key = key,
                Group = group,
                Description = description
            };
            _dbContext.SiteSettings.Add(setting);
        }

        setting.Value = CleanOptional(value) ?? string.Empty;
    }

    // ------------------------------------------------------------------
    // Bài viết
    // ------------------------------------------------------------------

    public async Task<int> SaveArticleAsync(PostFormViewModel form, CancellationToken cancellationToken)
    {
        var slug = await EnsureUniqueSlugAsync(
            _dbContext.Posts.AsNoTracking().Where(post => post.DeletedAt == null),
            SlugHelper.ToSlug(string.IsNullOrWhiteSpace(form.Slug) ? form.Title : form.Slug),
            form.Id,
            filterDeleted: false,
            cancellationToken);

        Post entity;
        if (form.Id == 0)
        {
            // Local seed database uses explicit post ids instead of an IDENTITY column.
            var nextId = await _dbContext.Posts.AsNoTracking().MaxAsync(post => (int?)post.Id, cancellationToken) + 1 ?? 1;
            entity = new Post { Id = nextId, CreatedAt = DateTime.UtcNow };
            _dbContext.Posts.Add(entity);
        }
        else
        {
            entity = await _dbContext.Posts.FirstOrDefaultAsync(post => post.Id == form.Id && post.DeletedAt == null, cancellationToken)
                ?? throw new InvalidOperationException($"Không tìm thấy bài viết #{form.Id}.");
        }

        entity.Title = form.Title.Trim();
        entity.Slug = slug;
        entity.PostCategoryId = form.PostCategoryId;
        entity.Excerpt = CleanOptional(form.Excerpt);
        entity.Content = CleanOptional(form.Content);
        entity.AuthorName = CleanOptional(form.AuthorName) ?? "Quản trị viên";
        entity.SeoKeywords = CleanOptional(form.SeoKeywords);
        entity.Status = form.Status == Draft ? Draft : Published;
        entity.SortOrder = form.SortOrder;
        entity.IsFeatured = form.IsFeatured;
        entity.PublishedAt = form.PublishedAt ?? DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        if (form.CoverFile is not null)
        {
            var upload = await _imageUpload.SaveAsWebpAsync(form.CoverFile, "post", entity.Slug, 1600, cancellationToken);
            if (!upload.Success)
            {
                throw new InvalidOperationException(upload.Error);
            }

            await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
            entity.CoverImage = upload.Url;
        }
        else if (form.RemoveCover)
        {
            await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
            entity.CoverImage = null;
        }

        var selectedProductIds = form.LinkedProductIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        var existingLinks = await _dbContext.PostProductLinks
            .Where(link => link.PostId == entity.Id)
            .ToListAsync(cancellationToken);
        _dbContext.PostProductLinks.RemoveRange(existingLinks);
        if (selectedProductIds.Count > 0)
        {
            var sortOrder = 1;
            foreach (var productId in selectedProductIds)
            {
                _dbContext.PostProductLinks.Add(new PostProductLink
                {
                    PostId = entity.Id,
                    ScrapItemId = productId,
                    SortOrder = sortOrder++
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await CleanupAutosaveAsync(form.AutosaveKey, entity.Id, cancellationToken);
        return entity.Id;
    }

    public async Task AutoSaveArticleDraftAsync(string postKey, PostFormViewModel form, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(postKey) || postKey.Length > 64)
        {
            throw new InvalidOperationException("Khóa tự lưu không hợp lệ.");
        }

        // ART-008: chặn race condition — sendBeacon lúc unload có thể đến SAU khi lưu chính thức
        // và ghi đè bản autosave vừa bị dọn. Nếu khóa này vừa được dọn sạch (trong cửa sổ ngắn)
        // thì bỏ qua lần ghi trễ đó để không còn sót nội dung stale trong PostAutosaves.
        if (_cache.TryGetValue(AutosaveClearedCacheKey(postKey), out _))
        {
            return;
        }

        var json = PostAutosavePayloadMapper.Serialize(PostAutosavePayloadMapper.FromForm(form));
        var autosave = await _dbContext.PostAutosaves.FirstOrDefaultAsync(item => item.PostKey == postKey, cancellationToken);
        if (autosave is null)
        {
            autosave = new PostAutosave { PostKey = postKey };
            _dbContext.PostAutosaves.Add(autosave);
        }

        autosave.DataJson = json;
        autosave.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Xóa bản nháp tự lưu sau khi nội dung đã được lưu chính thức.</summary>
    private async Task CleanupAutosaveAsync(string? autosaveKey, int postId, CancellationToken cancellationToken)
    {
        var keys = new List<string> { $"post-{postId}" };
        if (!string.IsNullOrWhiteSpace(autosaveKey))
        {
            keys.Add(autosaveKey.Trim());
        }

        var stale = await _dbContext.PostAutosaves
            .Where(item => keys.Contains(item.PostKey))
            .ToListAsync(cancellationToken);

        if (stale.Count > 0)
        {
            _dbContext.PostAutosaves.RemoveRange(stale);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Đánh dấu các khóa vừa dọn trong ~15 giây: beacon autosave gửi trễ trong cửa sổ này sẽ bị bỏ qua.
        foreach (var key in keys)
        {
            _cache.Set(AutosaveClearedCacheKey(key), DateTime.UtcNow, TimeSpan.FromSeconds(15));
        }
    }

    private static string AutosaveClearedCacheKey(string postKey) => $"admin:autosave-cleared:{postKey}";

    Task<bool> IAdminArticleCommandService.ToggleStatusAsync(int id, CancellationToken cancellationToken)
        => ToggleAsync(_dbContext.Posts, id, cancellationToken);

    Task<bool> IAdminArticleCommandService.ToggleFeaturedAsync(int id, CancellationToken cancellationToken)
        => ToggleFeaturedAsync(_dbContext.Posts, id, cancellationToken);

    public async Task<bool> DeleteArticleAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Posts.FindAsync([id], cancellationToken);
        if (entity is null || entity.DeletedAt != null)
        {
            return false;
        }

        // ART-010/011: giữ nguyên Status khi xóa mềm để Restore trả về đúng trạng thái trước xóa.
        // Public đã lọc DeletedAt == null nên bài published đã xóa không lộ ra ngoài.
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreArticleAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Posts.FindAsync([id], cancellationToken);
        if (entity is null || entity.DeletedAt is null)
        {
            return false;
        }

        entity.Slug = await EnsureUniqueSlugAsync(
            _dbContext.Posts.AsNoTracking().Where(post => post.DeletedAt == null),
            entity.Slug,
            entity.Id,
            filterDeleted: false,
            cancellationToken);
        entity.DeletedAt = null;
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PermanentDeleteArticleAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Posts
            .Include(post => post.Images)
            .FirstOrDefaultAsync(post => post.Id == id && post.DeletedAt != null, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        await _imageUpload.DeleteUploadedImageAsync(entity.CoverImage, cancellationToken);
        foreach (var image in entity.Images)
        {
            await _imageUpload.DeleteUploadedImageAsync(image.ImageUrl, cancellationToken);
        }

        _dbContext.PostProductLinks.RemoveRange(
            _dbContext.PostProductLinks.Where(link => link.PostId == entity.Id));

        _dbContext.Posts.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ------------------------------------------------------------------
    // Shared helpers
    // ------------------------------------------------------------------

    private async Task RemoveScrapBannerAsync(ScrapItem item, CancellationToken cancellationToken)
    {
        var banner = item.Images.FirstOrDefault(image => image.Caption == "banner");
        if (banner is null)
        {
            return;
        }

        await _imageUpload.DeleteUploadedImageAsync(banner.ImageUrl, cancellationToken);
        _dbContext.ScrapImages.Remove(banner);
    }

    private static string? CleanOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return System.Net.Mail.MailAddress.TryCreate(value, out _);
    }

    private async Task<string> EnsureUniqueSlugAsync<T>(
        IQueryable<T> query,
        string slug,
        int currentId,
        bool filterDeleted,
        CancellationToken cancellationToken) where T : class
    {
        if (filterDeleted)
        {
            // Soft-deleted rows keep their slug but no longer block reuse (matches filtered unique indexes).
            query = query.Where(entity => EF.Property<DateTime?>(entity, "DeletedAt") == null);
        }

        var candidate = slug;
        var suffix = 2;
        while (await query.AnyAsync(entity =>
                   EF.Property<string>(entity, "Slug") == candidate &&
                   EF.Property<int>(entity, "Id") != currentId,
               cancellationToken))
        {
            candidate = $"{slug}-{suffix++}";
        }

        return candidate;
    }

    private async Task<bool> ToggleAsync<T>(DbSet<T> set, int id, CancellationToken cancellationToken) where T : class
    {
        var entity = await set.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var entry = set.Entry(entity).Property("Status");
        entry.CurrentValue = (string)entry.CurrentValue! == Published ? Draft : Published;
        if (typeof(T).GetProperty("UpdatedAt") is not null)
        {
            set.Entry(entity).Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> ToggleFeaturedAsync<T>(DbSet<T> set, int id, CancellationToken cancellationToken) where T : class
    {
        var entity = await set.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var entry = set.Entry(entity).Property("IsFeatured");
        entry.CurrentValue = !(bool)entry.CurrentValue!;
        if (typeof(T).GetProperty("UpdatedAt") is not null)
        {
            set.Entry(entity).Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// "Insert at position" ordering: đặt mục vào vị trí mong muốn (1 = đầu danh sách)
    /// rồi đánh lại số liên tục 1..n cho toàn bộ tập — không bao giờ trùng thứ tự.
    /// </summary>
    private async Task<bool> RenumberSortAsync<T>(DbSet<T> set, int id, int position, CancellationToken cancellationToken) where T : class
    {
        if (position < 1)
        {
            position = 1;
        }

        var items = await set
            .OrderBy(item => EF.Property<int>(item, "SortOrder"))
            .ThenBy(item => EF.Property<int>(item, "Id"))
            .ToListAsync(cancellationToken);
        // EF.Property chỉ hợp lệ trong query; sau khi về bộ nhớ thì đọc qua change tracker.
        var target = items.FirstOrDefault(item => (int)set.Entry(item).Property("Id").CurrentValue! == id);
        if (target is null)
        {
            return false;
        }

        items.Remove(target);
        var index = Math.Clamp(position - 1, 0, items.Count);
        items.Insert(index, target);

        var order = 1;
        foreach (var item in items)
        {
            set.Entry(item).Property("SortOrder").CurrentValue = order++;
            if (typeof(T).GetProperty("UpdatedAt") is not null)
            {
                set.Entry(item).Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> UpdateSortAsync<T>(DbSet<T> set, int id, int sortOrder, CancellationToken cancellationToken) where T : class
    {
        if (sortOrder is < 0 or > 9999)
        {
            return false;
        }

        var entity = await set.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        set.Entry(entity).Property("SortOrder").CurrentValue = sortOrder;
        if (typeof(T).GetProperty("UpdatedAt") is not null)
        {
            set.Entry(entity).Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> SoftDeleteAsync<T>(DbSet<T> set, int id, CancellationToken cancellationToken) where T : class
    {
        var entity = await set.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        set.Entry(entity).Property("DeletedAt").CurrentValue = DateTime.UtcNow;
        set.Entry(entity).Property("Status").CurrentValue = Draft;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
