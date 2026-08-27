using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels;
using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.Services.Public;

public class PublicScrapQueryService : IPublicScrapQueryService
{
    private const int DefaultPageSize = 12;
    private const int MaxPageSize = 24;
    private readonly AppDbContext _dbContext;
    private readonly IPublicSeoQueryService _seoQueryService;

    public PublicScrapQueryService(AppDbContext dbContext, IPublicSeoQueryService seoQueryService)
    {
        _dbContext = dbContext;
        _seoQueryService = seoQueryService;
    }

    public async Task<ScrapIndexViewModel> GetScrapIndexAsync(ScrapListQueryDto query, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? DefaultPageSize : query.PageSize, 1, MaxPageSize);
        var pageNumber = Math.Max(1, query.PageNumber);

        var baseQuery = _dbContext.ScrapItems
            .AsNoTracking()
            .Where(item => item.Status == PublicConstants.Published && item.DeletedAt == null);

        var totalItems = await baseQuery.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        pageNumber = Math.Min(pageNumber, totalPages);

        var rows = await baseQuery
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
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows
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

        return new ScrapIndexViewModel
        {
            Seo = await _seoQueryService.GetByRouteAsync(
                "/phe-lieu",
                new SeoDto("Mặt hàng phế liệu", "Danh sách các mặt hàng phế liệu thu mua.", CanonicalUrl: "/phe-lieu"),
                cancellationToken),
            Page = new NumberedPageDto<ScrapCardDto>(items, pageNumber, pageSize, totalItems, totalPages)
        };
    }

    public async Task<IReadOnlyList<CategoryGroupCardDto>> GetCategoryGroupsAsync(CancellationToken cancellationToken)
    {
        var categoryRows = await _dbContext.ScrapCategories
            .AsNoTracking()
            .Where(category => category.Status == PublicConstants.Published)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Id)
            .Select(category => new
            {
                category.Id,
                category.Name,
                category.Slug,
                ItemCount = category.ScrapItems.Count(item => item.Status == PublicConstants.Published && item.DeletedAt == null),
                ImageUrl = category.ScrapItems
                    .Where(item => item.Status == PublicConstants.Published && item.DeletedAt == null)
                    .OrderByDescending(item => item.IsFeatured)
                    .ThenBy(item => item.SortOrder)
                    .Select(item => item.PrimaryImage)
                    .FirstOrDefault(),
                MinPriceFrom = category.ScrapItems
                    .Where(item => item.Status == PublicConstants.Published && item.DeletedAt == null && item.PriceFrom > 0)
                    .Select(item => item.PriceFrom)
                    .Min()
            })
            .ToListAsync(cancellationToken);

        var categoryItemNames = await _dbContext.ScrapItems
            .AsNoTracking()
            .Where(item => item.Status == PublicConstants.Published && item.DeletedAt == null && item.ScrapCategoryId != null)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .Select(item => new { item.ScrapCategoryId, item.Name })
            .ToListAsync(cancellationToken);

        return categoryRows
            .Select(category =>
            {
                var names = categoryItemNames
                    .Where(item => item.ScrapCategoryId == category.Id)
                    .Select(item => item.Name.Replace("Thu mua ", string.Empty, StringComparison.OrdinalIgnoreCase))
                    .Take(3)
                    .ToList();
                var more = category.ItemCount - names.Count;
                if (more > 0)
                {
                    names.Add($"+{more} loại khác");
                }
                return new CategoryGroupCardDto(
                    category.Id,
                    category.Name,
                    category.Slug,
                    category.ItemCount,
                    category.ImageUrl,
                    string.Join(" · ", names),
                    category.MinPriceFrom);
            })
            .Where(category => category.ItemCount > 0)
            .ToList();
    }

    public async Task<ScrapCategoryPageViewModel?> GetScrapCategoryPageAsync(string? slug, CancellationToken cancellationToken)
    {
        var groups = await GetCategoryGroupsAsync(cancellationToken);
        var model = new ScrapCategoryPageViewModel { Groups = groups };

        if (string.IsNullOrWhiteSpace(slug))
        {
            model.Seo = await _seoQueryService.GetByRouteAsync(
                "/phe-lieu/danh-muc",
                new SeoDto("Danh mục phế liệu thu mua", "Danh mục phế liệu thu mua theo nhóm chất liệu.", CanonicalUrl: "/phe-lieu/danh-muc"),
                cancellationToken);
            return model;
        }

        var current = await _dbContext.ScrapCategories
            .AsNoTracking()
            .Where(category => category.Status == PublicConstants.Published && category.Slug == slug)
            .Select(category => new { category.Id, category.Name, category.Slug, category.Description })
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            return null;
        }

        var itemRows = await _dbContext.ScrapItems
            .AsNoTracking()
            .Where(item => item.Status == PublicConstants.Published && item.DeletedAt == null && item.ScrapCategoryId == current.Id)
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
            .ToListAsync(cancellationToken);

        model.Current = groups.FirstOrDefault(group => group.Slug == current.Slug)
            ?? new CategoryGroupCardDto(current.Id, current.Name, current.Slug, itemRows.Count, null, string.Empty, null);
        model.CurrentDescription = current.Description;
        model.Items = itemRows
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

        model.Seo = await _seoQueryService.GetByRouteAsync(
            $"/phe-lieu/nhom/{current.Slug}",
            new SeoDto(
                $"Thu mua phế liệu {current.Name} giá cao",
                $"Danh sách {model.Items.Count} loại phế liệu {current.Name} đang thu mua với giá tham khảo theo kg.",
                CanonicalUrl: $"/phe-lieu/nhom/{current.Slug}"),
            cancellationToken);

        return model;
    }

    public async Task<ScrapDetailViewModel?> GetScrapDetailAsync(string slug, CancellationToken cancellationToken)
    {
        var item = await _dbContext.ScrapItems
            .AsNoTracking()
            .Where(scrap => scrap.Status == PublicConstants.Published && scrap.DeletedAt == null && scrap.Slug == slug)
            .Select(scrap => new
            {
                scrap.Id,
                scrap.Name,
                scrap.Slug,
                CategoryName = scrap.Category == null ? "Phế liệu" : scrap.Category.Name,
                scrap.ShortDescription,
                scrap.Description,
                scrap.PrimaryImage,
                scrap.PriceFrom,
                scrap.PriceLabel,
                scrap.Unit
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var gallery = await _dbContext.ScrapImages
            .AsNoTracking()
            .Where(image => image.ScrapItemId == item.Id)
            .OrderBy(image => image.OrderIndex)
            .ThenBy(image => image.Id)
            .Select(image => new ScrapGalleryImageDto(image.ImageUrl, image.Caption))
            .Take(4)
            .ToListAsync(cancellationToken);

        var relatedRows = await _dbContext.ScrapItems
            .AsNoTracking()
            .Where(scrap => scrap.Status == PublicConstants.Published && scrap.DeletedAt == null && scrap.Id != item.Id)
            .OrderByDescending(scrap => scrap.IsFeatured)
            .ThenBy(scrap => scrap.SortOrder)
            .ThenByDescending(scrap => scrap.PublishedAt)
            .ThenByDescending(scrap => scrap.Id)
            .Select(scrap => new
            {
                scrap.Id,
                scrap.Name,
                scrap.Slug,
                CategoryName = scrap.Category == null ? "Phế liệu" : scrap.Category.Name,
                scrap.ShortDescription,
                scrap.PrimaryImage,
                scrap.PriceFrom,
                scrap.PriceLabel,
                scrap.Unit,
                scrap.IsFeatured,
                scrap.SortOrder,
                scrap.PublishedAt
            })
            .Take(4)
            .ToListAsync(cancellationToken);

        var related = relatedRows
            .Select(scrap => new ScrapCardDto(
                scrap.Id,
                scrap.Name,
                scrap.Slug,
                scrap.CategoryName,
                scrap.ShortDescription,
                scrap.PrimaryImage,
                PriceTextBuilder.Build(scrap.PriceFrom, scrap.PriceLabel, scrap.Unit),
                scrap.Unit,
                scrap.IsFeatured,
                scrap.SortOrder,
                scrap.PublishedAt))
            .ToList();

        var detail = new ScrapDetailDto(
            item.Id,
            item.Name,
            item.Slug,
            item.CategoryName,
            item.ShortDescription,
            item.Description,
            item.PrimaryImage,
            PriceTextBuilder.Build(item.PriceFrom, item.PriceLabel, item.Unit),
            item.Unit,
            gallery);

        return new ScrapDetailViewModel
        {
            Seo = await _seoQueryService.GetByEntityAsync(
                "ScrapItem",
                item.Id,
                $"/phe-lieu/{item.Slug}",
                new SeoDto(item.Name, item.ShortDescription ?? "Chi tiết mặt hàng phế liệu.", CanonicalUrl: $"/phe-lieu/{item.Slug}", OgImage: item.PrimaryImage),
                cancellationToken),
            Item = detail,
            RelatedItems = related
        };
    }
}
