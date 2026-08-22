namespace ScrapWebsite.ViewModels.Public;

public sealed record BannerDto(
    string Title,
    string? Subtitle,
    string? ImageUrl,
    string? PrimaryButtonText,
    string? PrimaryButtonUrl,
    string? SecondaryButtonText,
    string? SecondaryButtonUrl);
