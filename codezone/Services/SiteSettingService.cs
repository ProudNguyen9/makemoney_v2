using ScrapWebsite.Data;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services;

public class SiteSettingService : ISiteSettingService
{
    public async Task<HomeViewModel> GetHomeAsync()
    {
        return new HomeViewModel
        {
            Seo = await GetSeoAsync("home"),
            HeroBanner = await GetHeroBannerAsync(),
            FeaturedScrapItems = SeedData.FeaturedScrapItems,
            LatestPosts = SeedData.LatestPosts
        };
    }

    public Task<Banner> GetHeroBannerAsync()
    {
        return Task.FromResult(new Banner
        {
            Title = "Thu mua phe lieu gia cao",
            Subtitle = "Khung MVC rieng san sang migrate template tung phan.",
            ImageUrl = "/images/shared/placeholder.svg",
            PrimaryButtonText = "Lien he bao gia",
            PrimaryButtonUrl = "/Contact"
        });
    }

    public Task<SharedSeoViewModel> GetSeoAsync(string pageKey)
    {
        var title = pageKey.Equals("home", StringComparison.OrdinalIgnoreCase)
            ? "Trang chu"
            : "ScrapWebsite";

        return Task.FromResult(new SharedSeoViewModel
        {
            Title = title,
            Description = "Website thu mua phe lieu xay dung bang ASP.NET Core MVC 10."
        });
    }
}
