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
    DateTime PublishedAt);

public sealed record PostImageDto(
    string ImageUrl,
    string? Caption);

public sealed record PostListQueryDto(
    int PageSize = 10,
    string? Cursor = null,
    string? PreviousCursor = null,
    int PageNumber = 1);
