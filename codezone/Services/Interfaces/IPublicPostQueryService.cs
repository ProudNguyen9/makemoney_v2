using ScrapWebsite.ViewModels;
using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.Services.Interfaces;

public interface IPublicPostQueryService
{
    Task<NewsIndexViewModel> GetNewsIndexAsync(PostListQueryDto query, CancellationToken cancellationToken);

    Task<NewsDetailViewModel?> GetNewsDetailAsync(string slug, CancellationToken cancellationToken);
}
