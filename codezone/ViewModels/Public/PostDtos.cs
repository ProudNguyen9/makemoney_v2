namespace ScrapWebsite.ViewModels.Public;

public sealed record PostCardDto(
    int Id,
    string Title,
    string Slug,
    string CategoryName,
    string? Excerpt,
    string? CoverImage,
    DateTime PublishedAt,
    bool IsFeatured,
    int SortOrder);

public sealed record PostDetailDto(
    int Id,
    string Title,
    string Slug,
    string CategoryName,
    string? Excerpt,
    string? ContentHtml,
    string? CoverImage,
    string AuthorName,
    DateTime PublishedAt,
    string? SeoKeywords = null,
    bool IsDraft = false);

public sealed record PostLinkedProductDto(
    int Id,
    string Name,
    string Slug,
    string CategoryName,
    string? ImageUrl,
    string? PriceText,
    string? ShortDescription);

public sealed record PostImageDto(
    string ImageUrl,
    string? Caption);

public sealed record PostListQueryDto(
    int PageSize = 10,
    string? Cursor = null,
    string? PreviousCursor = null,
    int PageNumber = 1);
