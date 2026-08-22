using ScrapWebsite.ViewModels;
using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.Services.Interfaces;

public interface IPublicScrapQueryService
{
    Task<ScrapIndexViewModel> GetScrapIndexAsync(ScrapListQueryDto query, CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryGroupCardDto>> GetCategoryGroupsAsync(CancellationToken cancellationToken);

    Task<ScrapCategoryPageViewModel?> GetScrapCategoryPageAsync(string? slug, CancellationToken cancellationToken);

    Task<ScrapDetailViewModel?> GetScrapDetailAsync(string slug, CancellationToken cancellationToken);
}
