using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services.Interfaces;

public interface IScrapService
{
    Task<ScrapIndexViewModel> GetIndexAsync();

    Task<ScrapDetailViewModel?> GetDetailAsync(string slug);
}
