namespace ScrapWebsite.ViewModels.Public;

public sealed record CursorPageLinkDto(
    string Label,
    string? Cursor,
    string? PreviousCursor,
    int PageNumber,
    bool IsCurrent,
    bool IsDisabled,
    string? Rel = null);

public sealed record CursorPageDto<T>(
    IReadOnlyList<T> Items,
    string? NextCursor,
    bool HasMore)
{
    public int PageNumber { get; init; } = 1;

    public string? PreviousCursor { get; init; }

    public IReadOnlyList<CursorPageLinkDto> Links { get; init; } = [];
}
