using ScrapWebsite.ViewModels.Public;

namespace codezone.ViewModels.Shared;

public sealed record SiteChromeViewModel(
    string CompanyName,
    string Hotline,
    string HotlineHref,
    string Email,
    string Zalo,
    string ZaloHref,
    string Address,
    string WarehouseAddress,
    string WorkingHours,
    string TaxCode,
    string LogoUrl,
    string FooterLogoUrl,
    string PurchaseAreas,
    string DefaultHeroImage,
    string DefaultCtaImage,
    string ResponseTimeText)
{
    public IReadOnlyList<CategoryGroupCardDto> ScrapCategories { get; init; } = [];
}
