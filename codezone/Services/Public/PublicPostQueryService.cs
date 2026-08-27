using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Services.Admin;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels;
using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.Services.Public;

public class PublicPostQueryService : IPublicPostQueryService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 20;
    private readonly AppDbContext _dbContext;
    private readonly IPublicSeoQueryService _seoQueryService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PublicPostQueryService(AppDbContext dbContext, IPublicSeoQueryService seoQueryService, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _seoQueryService = seoQueryService;
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsAdminRequest =>
        _httpContextAccessor.HttpContext?.User.Identity is { IsAuthenticated: true } identity
        && identity.AuthenticationType == AdminAuthDefaults.AuthenticationScheme;

    public async Task<NewsIndexViewModel> GetNewsIndexAsync(PostListQueryDto query, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? DefaultPageSize : query.PageSize, 1, MaxPageSize);
        var cursor = CursorToken.Decode(query.Cursor);
        var postQuery = _dbContext.Posts
            .AsNoTracking()
            .Where(post => post.Status == PublicConstants.Published && post.DeletedAt == null);

        if (cursor is not null)
        {
            postQuery = postQuery.Where(post =>
                (cursor.IsFeatured && !post.IsFeatured) ||
                (post.IsFeatured == cursor.IsFeatured && post.SortOrder > cursor.SortOrder) ||
                (post.IsFeatured == cursor.IsFeatured && post.SortOrder == cursor.SortOrder && post.PublishedAt < (cursor.PublishedAt ?? DateTime.MinValue)) ||
                (post.IsFeatured == cursor.IsFeatured && post.SortOrder == cursor.SortOrder && post.PublishedAt == (cursor.PublishedAt ?? DateTime.MinValue) && post.Id < cursor.Id));
        }

        var rows = await postQuery
            .OrderByDescending(post => post.IsFeatured)
            .ThenBy(post => post.SortOrder)
            .ThenByDescending(post => post.PublishedAt)
            .ThenByDescending(post => post.Id)
            .Select(post => new PostCardDto(
                post.Id,
                post.Title,
                post.Slug,
                post.Category == null ? "Tin tức" : post.Category.Name,
                post.Excerpt,
                post.CoverImage,
                post.PublishedAt,
                post.IsFeatured,
                post.SortOrder))
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var items = rows.Take(pageSize).ToList();
        var nextCursor = rows.Count > pageSize && items.Count > 0
            ? CursorToken.Encode(new PublicCursor(items[^1].IsFeatured, items[^1].SortOrder, items[^1].PublishedAt, items[^1].Id))
            : null;
        var sidebarPosts = items.Count <= 5 ? items : items.GetRange(0, 5);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var paginationLinks = await BuildPaginationLinksAsync(query.Cursor, query.PreviousCursor, pageNumber, pageSize, nextCursor, cancellationToken);
        var chromeSettings = await _dbContext.SiteSettings
            .AsNoTracking()
            .Where(setting => setting.Key == "news.hero_image" ||
                              setting.Key == "news.hero_title" ||
                              setting.Key == "news.hero_description")
            .Select(setting => new { setting.Key, setting.Value })
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value ?? string.Empty, cancellationToken);

        return new NewsIndexViewModel
        {
            Seo = await _seoQueryService.GetByRouteAsync(
                "/tin-tuc",
                new SeoDto("Tin tức", "Tin tức và kinh nghiệm thu mua phế liệu.", CanonicalUrl: "/tin-tuc"),
                cancellationToken),
            Chrome = new NewsChromeDto(
                GetSetting(chromeSettings, "news.hero_image", "/assets/images/imported/brand/seo-og-image.png"),
                GetSetting(chromeSettings, "news.hero_title", "Tin tức & kiến thức phế liệu"),
                GetSetting(chromeSettings, "news.hero_description", "Cập nhật giá phế liệu, kinh nghiệm thanh lý và thông tin thu mua mới nhất.")),
            PageSize = pageSize,
            Page = new CursorPageDto<PostCardDto>(items, nextCursor, nextCursor is not null)
            {
                PageNumber = pageNumber,
                PreviousCursor = query.PreviousCursor,
                Links = paginationLinks
            },
            SidebarPosts = sidebarPosts
        };
    }

    public async Task<NewsDetailViewModel?> GetNewsDetailAsync(string slug, CancellationToken cancellationToken)
    {
        var postQuery = _dbContext.Posts
            .AsNoTracking()
            .Where(item => item.DeletedAt == null && item.Slug == slug);

        if (!IsAdminRequest)
        {
            postQuery = postQuery.Where(item => item.Status == PublicConstants.Published);
        }

        var post = await postQuery
            .Select(item => new PostDetailDto(
                item.Id,
                item.Title,
                item.Slug,
                item.Category == null ? "Tin tức" : item.Category.Name,
                item.Excerpt,
                item.Content,
                item.CoverImage,
                item.AuthorName ?? "Quản trị viên",
                item.PublishedAt,
                item.SeoKeywords,
                IsDraft: item.Status != PublicConstants.Published))
            .FirstOrDefaultAsync(cancellationToken);

        if (post is null)
        {
            return null;
        }

        var images = await _dbContext.PostImages
            .AsNoTracking()
            .Where(image => image.PostId == post.Id)
            .OrderBy(image => image.OrderIndex)
            .ThenBy(image => image.Id)
            .Select(image => new PostImageDto(image.ImageUrl, image.Caption))
            .Take(6)
            .ToListAsync(cancellationToken);

        var linkedProducts = await _dbContext.PostProductLinks
            .AsNoTracking()
            .Where(link => link.PostId == post.Id)
            .OrderBy(link => link.SortOrder)
            .ThenBy(link => link.Id)
            .Select(link => new PostLinkedProductDto(
                link.ScrapItemId,
                link.ScrapItem != null ? link.ScrapItem.Name : "Sản phẩm liên kết",
                link.ScrapItem != null ? link.ScrapItem.Slug : "",
                link.ScrapItem != null && link.ScrapItem.Category != null ? link.ScrapItem.Category.Name : "Sản phẩm",
                link.ScrapItem != null ? link.ScrapItem.PrimaryImage : null,
                link.ScrapItem != null ? (link.ScrapItem.PriceLabel ?? (link.ScrapItem.PriceFrom.HasValue ? $"{link.ScrapItem.PriceFrom.Value:N0} đ/{(link.ScrapItem.Unit ?? "kg")}" : null)) : null,
                link.ScrapItem != null ? link.ScrapItem.ShortDescription : null))
            .ToListAsync(cancellationToken);

        var related = await _dbContext.Posts
            .AsNoTracking()
            .Where(item => item.Status == PublicConstants.Published && item.DeletedAt == null && item.Id != post.Id)
            .OrderByDescending(item => item.PublishedAt)
            .ThenByDescending(item => item.Id)
            .Select(item => new PostCardDto(
                item.Id,
                item.Title,
                item.Slug,
                item.Category == null ? "Tin tức" : item.Category.Name,
                item.Excerpt,
                item.CoverImage,
                item.PublishedAt,
                item.IsFeatured,
                item.SortOrder))
            .Take(3)
            .ToListAsync(cancellationToken);

        return new NewsDetailViewModel
        {
            Seo = await _seoQueryService.GetByEntityAsync(
                "Post",
                post.Id,
                $"/tin-tuc/{post.Slug}",
                new SeoDto(post.Title, post.Excerpt ?? "Chi tiết tin tức.", post.SeoKeywords, CanonicalUrl: $"/tin-tuc/{post.Slug}", OgImage: post.CoverImage, OgType: "article"),
                cancellationToken),
            Post = post,
            Images = images,
            LinkedProducts = linkedProducts,
            RelatedPosts = related
        };
    }

    private static string GetSetting(IReadOnlyDictionary<string, string> settings, string key, string fallback)
    {
        return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : fallback;
    }

    private async Task<IReadOnlyList<CursorPageLinkDto>> BuildPaginationLinksAsync(
        string? currentCursor,
        string? previousCursor,
        int currentPage,
        int pageSize,
        string? nextCursor,
        CancellationToken cancellationToken)
    {
        var links = new List<CursorPageLinkDto>();
        links.Add(new CursorPageLinkDto("Trước", previousCursor, null, Math.Max(currentPage - 1, 1), false, currentPage <= 1, "prev"));
        links.Add(new CursorPageLinkDto(currentPage.ToString(), currentCursor, previousCursor, currentPage, true, false));

        var cursorForPage = nextCursor;
        var previousForPage = currentCursor;
        for (var page = currentPage + 1; page <= currentPage + 4 && !string.IsNullOrWhiteSpace(cursorForPage); page++)
        {
            links.Add(new CursorPageLinkDto(page.ToString(), cursorForPage, previousForPage, page, false, false));
            previousForPage = cursorForPage;
            cursorForPage = await GetNextCursorAsync(cursorForPage, pageSize, cancellationToken);
        }

        links.Add(new CursorPageLinkDto("Tiếp", nextCursor, currentCursor, currentPage + 1, false, string.IsNullOrWhiteSpace(nextCursor), "next"));
        return links;
    }

    private async Task<string?> GetNextCursorAsync(string? cursorToken, int pageSize, CancellationToken cancellationToken)
    {
        var cursor = CursorToken.Decode(cursorToken);
        if (cursor is null)
        {
            return null;
        }

        var rows = await ApplyCursor(_dbContext.Posts.AsNoTracking().Where(post => post.Status == PublicConstants.Published && post.DeletedAt == null), cursor)
            .OrderByDescending(post => post.IsFeatured)
            .ThenBy(post => post.SortOrder)
            .ThenByDescending(post => post.PublishedAt)
            .ThenByDescending(post => post.Id)
            .Select(post => new PublicCursor(post.IsFeatured, post.SortOrder, post.PublishedAt, post.Id))
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var pageRows = rows.Take(pageSize).ToList();
        return rows.Count > pageSize && pageRows.Count > 0
            ? CursorToken.Encode(pageRows[^1])
            : null;
    }

    private static IQueryable<Models.Post> ApplyCursor(IQueryable<Models.Post> query, PublicCursor cursor)
    {
        return query.Where(post =>
            (cursor.IsFeatured && !post.IsFeatured) ||
            (post.IsFeatured == cursor.IsFeatured && post.SortOrder > cursor.SortOrder) ||
            (post.IsFeatured == cursor.IsFeatured && post.SortOrder == cursor.SortOrder && post.PublishedAt < (cursor.PublishedAt ?? DateTime.MinValue)) ||
            (post.IsFeatured == cursor.IsFeatured && post.SortOrder == cursor.SortOrder && post.PublishedAt == (cursor.PublishedAt ?? DateTime.MinValue) && post.Id < cursor.Id));
    }
}
