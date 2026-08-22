namespace codezone.ViewModels.Shared;

public sealed record PageHeroViewModel(
    string Label,
    string Title,
    string Description,
    string BackgroundImage,
    string TitleId,
    IReadOnlyList<(string Text, string? Url)> BreadcrumbItems);
