using ScrapWebsite.Models;
using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services.Interfaces;

public interface ISiteSettingService
{
    Task<HomeViewModel> GetHomeAsync();

    Task<Banner> GetHeroBannerAsync();

    Task<SharedSeoViewModel> GetSeoAsync(string pageKey);
}
