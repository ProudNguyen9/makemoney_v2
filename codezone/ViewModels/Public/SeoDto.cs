namespace ScrapWebsite.ViewModels.Public;

public sealed record SeoDto(
    string Title,
    string Description,
    string? Keywords = null,
    string? CanonicalUrl = null,
    string? OgTitle = null,
    string? OgDescription = null,
    string? OgImage = null,
    string OgType = "website",
    bool RobotsIndex = true,
    bool RobotsFollow = true);
