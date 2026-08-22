namespace codezone.ViewModels.Shared;

public sealed record SectionHeadViewModel(
    string Label,
    string Title,
    string? Description = null,
    string? TitleId = null,
    string CssClass = "sec-head");
