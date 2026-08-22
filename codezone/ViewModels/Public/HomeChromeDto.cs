namespace ScrapWebsite.ViewModels.Public;

public sealed record HomeChromeDto(
    string Hotline,
    string HotlineHref,
    string ZaloHref,
    string PriceUpdatedText,
    string ResponseTimeText,
    IReadOnlyList<string> PurchaseAreas,
    string AboutImageMain,
    string AboutImageTruck,
    string AboutImageScale,
    string ProjectImage1,
    string ProjectImage2,
    string ProjectImage3,
    string ReferralImage,
    string FinalCtaImage);
