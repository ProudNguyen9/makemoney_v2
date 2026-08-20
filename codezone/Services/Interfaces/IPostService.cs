using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services.Interfaces;

public interface IPostService
{
    Task<NewsIndexViewModel> GetIndexAsync();

    Task<NewsDetailViewModel?> GetDetailAsync(string slug);
}
