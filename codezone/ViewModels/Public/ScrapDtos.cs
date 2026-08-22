namespace ScrapWebsite.ViewModels.Public;

public sealed record ScrapCardDto(
    int Id,
    string Name,
    string Slug,
    string CategoryName,
    string? ShortDescription,
    string? ImageUrl,
    string PriceText,
    string? Unit,
    bool IsFeatured,
    int SortOrder,
    DateTime? PublishedAt);

public sealed record ScrapGalleryImageDto(
    string ImageUrl,
    string? Caption);

public sealed record ScrapDetailDto(
    int Id,
    string Name,
    string Slug,
    string CategoryName,
    string? ShortDescription,
    string? DescriptionHtml,
    string? ImageUrl,
    string PriceText,
    string? Unit,
    IReadOnlyList<ScrapGalleryImageDto> GalleryImages);

public sealed record ScrapListQueryDto(int PageNumber = 1, int PageSize = 12);

public sealed record CategoryGroupCardDto(
    int Id,
    string Name,
    string Slug,
    int ItemCount,
    string? ImageUrl,
    string SampleText,
    decimal? MinPriceFrom);

public sealed record NumberedPageDto<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages)
{
    public bool HasPrevious => PageNumber > 1;

    public bool HasNext => PageNumber < TotalPages;
}
